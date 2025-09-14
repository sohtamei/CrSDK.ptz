// "get live view with http and ptz(for c#)" sample
#include <chrono>
#include <cinttypes>
#include <cstdint>
#include <cstdlib>
#include <fstream>
#include <future>
#include <iostream>
#include <mutex>
#include <sstream>
#include <stdexcept>
#include <string>
#include <thread>
#include <vector>

#if !defined(__APPLE__)
  #if defined(USE_EXPERIMENTAL_FS) // for jetson
    #include <experimental/filesystem>
    namespace fs = std::experimental::filesystem;
  #else
    #include <filesystem>
    namespace fs = std::filesystem;
  #endif
#endif

#if defined(__APPLE__) || defined(__linux__)
  #include <unistd.h>
#endif

// macro for multibyte character
#if defined(_WIN32) || defined(_WIN64)
  using CrString = std::wstring;
  #define CRSTR(s) L ## s
  #define CrCout std::wcerr
  #define DELIMITER CRSTR("\\")
#else
  using CrString = std::string;
  #define CRSTR(s) s
  #define CrCout std::cerr
  #define DELIMITER CRSTR("/")
#endif


#include "CRSDK/CrDeviceProperty.h"
#include "CRSDK/CameraRemote_SDK.h"
#include "CRSDK/IDeviceCallback.h"
#include "CrDebugString.h"   // use CrDebugString.cpp
#include "RemoteCli.h"

#define PrintError(msg, err) { fprintf(stderr, "Error in %s(%d):" msg ",%s\n", __FUNCTION__, __LINE__, (err ? CrErrorString(err).c_str():"")); }
#define GotoError(msg, err) { PrintError(msg, err); goto Error; }

bool  m_connected = false;
bool  m_disconnect_req = false;
std::string m_modelId;
int64_t  m_device_handle = 0;

std::mutex m_eventPromiseMutex;
uint32_t m_setDPCode = 0;
std::promise<void>* m_eventPromise = nullptr;
void setEventPromise(std::promise<void>* dp)
{
    std::lock_guard<std::mutex> lock(m_eventPromiseMutex);
    m_eventPromise = dp;
}

LiveviewCbFunc m_liveviewCb = nullptr;
void RegisterLiveviewCb(LiveviewCbFunc liveviewCb)
{
    m_liveviewCb = liveviewCb;
}

/*
std::promise<void>* m_lvPromise = nullptr;
std::mutex m_lvPromiseMutex;
void setLvPromise(std::promise<void>* dp)
{
    std::lock_guard<std::mutex> lock(m_lvPromiseMutex);
    m_lvPromise = dp;
}
*/
SCRSDK::CrError _getDeviceProperty(int64_t device_handle, uint32_t code, SCRSDK::CrDeviceProperty* devProp)
{
    std::int32_t nprop = 0;
    SCRSDK::CrDeviceProperty* prop_list = nullptr;
    SCRSDK::CrError err = SCRSDK::GetSelectDeviceProperties(device_handle, 1, &code, &prop_list, &nprop);
    if(err) GotoError("", err);
    if(prop_list && nprop >= 1) {
        *devProp = prop_list[0];
    }
Error:
    if(prop_list) SCRSDK::ReleaseDeviceProperties(device_handle, prop_list);
    return err;
}

SCRSDK::CrError _setDeviceProperty(int64_t device_handle, uint32_t code, uint64_t data, bool blocking=true)
{
    int result = SCRSDK::CrError_Generic_Unknown;
    SCRSDK::CrError err = 0;
    std::promise<void> eventPromise;
    std::future<void> eventFuture = eventPromise.get_future();
    std::future_status status;

    SCRSDK::CrDeviceProperty devProp;

    err = _getDeviceProperty(device_handle, code, &devProp);
    if(err) GotoError("", err);
    if (blocking && devProp.GetCurrentValue() == data) {
        std::cerr << "skipped\n";
        return 0;
    }

    if(blocking) {
        std::lock_guard<std::mutex> lock(m_eventPromiseMutex);
        m_setDPCode = code;
        m_eventPromise = &eventPromise;
    }

    devProp.SetCurrentValue(data);
    err = SCRSDK::SetDeviceProperty(device_handle, &devProp);
    if(err) GotoError("", err);

    if(!blocking) return 0;

    status = eventFuture.wait_for(std::chrono::milliseconds(3000));
    if(status != std::future_status::ready) GotoError("timeout", 0);

    try{
        eventFuture.get();
    } catch(const std::exception&) GotoError("", 0);
    std::cerr << "OK\n";

    result = 0;
Error:
    setEventPromise(nullptr);
    return result;
}

class DeviceCallback : public SCRSDK::IDeviceCallback
{
public:
    DeviceCallback() {};
    ~DeviceCallback() {};

    void OnConnected(SCRSDK::DeviceConnectionVersioin version)
    {
        std::cerr << "Connected to " << m_modelId << "\n";
        m_connected = true;
        std::lock_guard<std::mutex> lock(m_eventPromiseMutex);
        if(m_eventPromise) {
            m_eventPromise->set_value();
            m_eventPromise = nullptr;
        }
    }

    void OnError(CrInt32u error)
    {
        fprintf(stderr, "Connection error:%s\n", CrErrorString(error).c_str());
        std::lock_guard<std::mutex> lock(m_eventPromiseMutex);
        if(m_eventPromise) {
            m_eventPromise->set_exception(std::make_exception_ptr(std::runtime_error("error")));
            m_eventPromise = nullptr;
        }
    }

    void OnDisconnected(CrInt32u error)
    {
        std::cerr << "Disconnected from " << m_modelId << "\n";
        m_connected = false;
        std::lock_guard<std::mutex> lock(m_eventPromiseMutex);
        if(m_eventPromise) {
            m_eventPromise->set_value();
            m_eventPromise = nullptr;
        }
    }

    void OnCompleteDownload(CrChar* filename, CrInt32u type )
    {
        CrCout << "OnCompleteDownload:" << filename << "\n";
    }

    void OnNotifyContentsTransfer(CrInt32u notify, SCRSDK::CrContentHandle contentHandle, CrChar* filename)
    {
        std::cerr << "OnNotifyContentsTransfer.\n";
    }

    void OnWarning(CrInt32u warning)
    {
        if (warning == SCRSDK::CrWarning_Connect_Reconnecting) {
            std::cerr << "Reconnecting to " << m_modelId << "\n";
            return;
        }
    }

    void OnWarningExt(CrInt32u warning, CrInt32 param1, CrInt32 param2, CrInt32 param3) {}
    void OnLvPropertyChanged() {}
    void OnLvPropertyChangedCodes(CrInt32u num, CrInt32u* codes) {}
    void OnPropertyChanged() {}
    void OnPropertyChangedCodes(CrInt32u num, CrInt32u* codes)
    {
        //std::cerr << "OnPropertyChangedCodes:\n";
        for(uint32_t i = 0; i < num; ++i) {
            std::lock_guard<std::mutex> lock(m_eventPromiseMutex);
            if(m_setDPCode && m_setDPCode == codes[i]) {
                m_setDPCode = 0;
                if(m_eventPromise) {
                    m_eventPromise->set_value();
                    m_eventPromise = nullptr;
                }
            }
/*
            std::string name = CrDevicePropertyString((SCRSDK::CrDevicePropertyCode)codes[i]);

            SCRSDK::CrDeviceProperty devProp;
            SCRSDK::CrError err = _getDeviceProperty(m_device_handle, codes[i], &devProp);
            if(err) break;
            if(devProp.GetValueType() == SCRSDK::CrDataType_STR) {
            //  CrCout << "  " << CrString(name.begin(), name.end()) << "=\"" << _getCurrentStr(&devProp) << "\"\n";
            } else {
                int64_t current = devProp.GetCurrentValue();
                if(current < 10) {
                    fprintf(stderr, "  %s=%d\n", name.c_str(), (int)current);
                } else {
                    fprintf(stderr, "  %s=0x%x(%d)\n", name.c_str(), (int)current, (int)current);
                }
            }
*/
        }
    }
    
    void OnNotifyMonitorUpdated(CrInt32u type, CrInt32u frameNo)
    {
        if(type == SCRSDK::CrMonitorUpdated_LiveView) {
    //  fprintf(stderr, "%x", frameNo & 0xF);
		    if(m_liveviewCb && !m_disconnect_req) m_liveviewCb(0);
        }
    }

};

std::vector<std::string> _split(std::string inputLine, char delimiter)
{
    std::vector<std::string> strArray;
    if (inputLine.empty()) return strArray;

    std::string tmp;
    std::stringstream ss{inputLine};
    while (getline(ss, tmp, delimiter)) {
        strArray.push_back(tmp);
    }
    return strArray;
}

DeviceCallback deviceCallback;

int RemoteCli_connect(char* inputLine) //<ipaddress> [userid] [pass]
{
    int result = -1;
    SCRSDK::CrError err = SCRSDK::CrError_None;
    SCRSDK::ICrEnumCameraObjectInfo* enumCameraObjectInfo = nullptr;
    SCRSDK::ICrCameraObjectInfo* objInfo = nullptr;

  #if defined(__APPLE__)
    #define MAC_MAX_PATH 255
    char pathBuf[MAC_MAX_PATH] = {0};
    if(NULL == getcwd(pathBuf, sizeof(pathBuf) - 1)) return 1;
    CrString path = pathBuf;
  #else
    CrString path = fs::current_path().native();
  #endif

    bool boolRet = SCRSDK::Init();
    if(!boolRet) GotoError("", 0);

    {
        uint32_t model = SCRSDK::CrCameraDeviceModel_BRC_AM7;
        std::string  userId = "";
        std::string  userPassword = "";
        CrInt8u macAddress[6] = {0};
        CrInt32u ipAddress = 0;
        bool SSHsupport = false;

        std::vector<std::string> args = _split(inputLine, ' ');
        if(args.size() < 1) GotoError("invalid input", 0);

        std::vector<std::string> ips = _split(args[0], '.');
        if(ips.size() < 4) GotoError("invalid input", 0);
        for(int i = 0; i < 4; i++) {
            try { ipAddress |= stoi(ips[i]) << (i*8); } catch(const std::exception&) { GotoError("invalid input", 0); }
        }

        if(args.size() >= 3) {
            SSHsupport = true;
            userId = args[1];
            userPassword = args[2];
        }
        err = SCRSDK::CreateCameraObjectInfoEthernetConnection(&objInfo, (SCRSDK::CrCameraDeviceModelList)model, ipAddress, macAddress, SSHsupport);
        if(err || objInfo == nullptr) GotoError("", err);

        m_modelId = args[0];

        // connect
        {
            char fpBuff[128] = {0};
            CrInt32u fpLen = 0;
            std::promise<void> eventPromise;
            std::future<void> eventFuture = eventPromise.get_future();

            if (objInfo->GetSSHsupport() == SCRSDK::CrSSHsupport_ON) {
                SCRSDK::CrError err = SCRSDK::GetFingerprint(objInfo, fpBuff, &fpLen);
                if(err) GotoError("", err);
                std::cerr << "fingerprint: " << fpBuff << "\n";
            }

            setEventPromise(&eventPromise);
            err = SCRSDK::Connect(objInfo, &deviceCallback, &m_device_handle,
                SCRSDK::CrSdkControlMode_Remote,
                SCRSDK::CrReconnecting_ON,
                userId.c_str(), userPassword.c_str(), fpBuff, fpLen);
            if(err) GotoError("", err);

        //  std::future_status status = eventFuture.wait_for(std::chrono::milliseconds(3000));
        //  if(status != std::future_status::ready) GotoError("timeout",0);
            try{
                eventFuture.get();
            } catch(const std::exception&) GotoError("", 0);
        }
    }

    // set work directory
    {
        CrCout << "path=" << path.data() << "\n";
        err = SCRSDK::SetSaveInfo(m_device_handle, const_cast<CrChar*>(path.data()), const_cast<CrChar*>(CRSTR("DSC")), -1/*startNo*/);
        if(err) GotoError("", err);
    }

    std::this_thread::sleep_for(std::chrono::milliseconds(1000));

    // set LiveViewProtocol=2(http)
    err = _setDeviceProperty(m_device_handle, SCRSDK::CrDeviceProperty_LiveViewProtocol, 2/*http*/);
    //if(err) goto Error;

    if(enumCameraObjectInfo) enumCameraObjectInfo->Release();
    return 0;
Error:
    if(enumCameraObjectInfo) enumCameraObjectInfo->Release();
    RemoteCli_disconnect();
    return result;
}

int RemoteCli_disconnect(void)
{
    if(m_connected) {
        m_disconnect_req = true;
        std::promise<void> eventPromise;
        std::future<void> eventFuture = eventPromise.get_future();
        setEventPromise(&eventPromise);
        SCRSDK::Disconnect(m_device_handle);
        std::future_status status = eventFuture.wait_for(std::chrono::milliseconds(3000));
        if(status != std::future_status::ready) PrintError("timeout",0);
    }
    if(m_device_handle) SCRSDK::ReleaseDevice(m_device_handle);
    SCRSDK::Release();

    m_disconnect_req = false;
    m_connected = false;
    m_device_handle = 0;
    fprintf(stderr, "xxx\n");
    return 0;
}

int setDeviceProperty(char* code, int64_t data, bool blocking=true)
{
    int32_t codeInt = CrDevicePropertyCode(code);
    if(codeInt < 0) {
        PrintError("unknown DP",0);
        return SCRSDK::CrError_Generic_Unknown;
    }
    return _setDeviceProperty(m_device_handle, codeInt, (uint64_t)data, blocking);
}

int sendCommand(char* inputLine)
{
    SCRSDK::CrError err = 0;
    int64_t data = 0;
    int32_t code = 0;
    std::vector<std::string> args = _split(inputLine, ' ');
    if(args.size() < 2) GotoError("invalid", 0);

    code = CrCommandIdCode(args[0]);
    if(code < 0) return -1;
    try{ data = stoi(args[1]); } catch(const std::exception&) GotoError("", 0);

    err = SCRSDK::SendCommand(m_device_handle, code, (SCRSDK::CrCommandParam)data);
    if(err) GotoError("", err);

    if(args.size() >= 3) {
	    std::this_thread::sleep_for(std::chrono::milliseconds(50));
		try{ data = stoi(args[2]); } catch(const std::exception&) GotoError("", 0);

	    err = SCRSDK::SendCommand(m_device_handle, code, (SCRSDK::CrCommandParam)data);
	    if(err) GotoError("", err);
    }
Error:
    return err;
}

int controlPTZF(char* inputLine)
{
    SCRSDK::CrError err = 0;
    std::vector<std::string> args = _split(inputLine, ' ');

    #define SPEED_MAX 50 // 127

    int type = 0;
    SCRSDK::CrPTZFSetting ptzfSetting;
    ptzfSetting.pan.exists = 1;
    ptzfSetting.pan.position = 0;
    ptzfSetting.pan.speed = SPEED_MAX;
    ptzfSetting.tilt.exists = 1;
    ptzfSetting.tilt.position = 0;
    ptzfSetting.tilt.speed = SPEED_MAX;

    if(args.size() <= 0) return -1;
    try { type = stoi(args[0]); } catch(const std::exception&) { GotoError("invalid input", 0); }

    if(args.size() >= 2) try { ptzfSetting.pan.position = stoi(args[1]); } catch(const std::exception&) { GotoError("invalid input", 0); }
    if(args.size() >= 3) try { ptzfSetting.tilt.position = stoi(args[2]); } catch(const std::exception&) { GotoError("invalid input", 0); }
    if(args.size() >= 4) try { ptzfSetting.pan.speed= stoi(args[3]); } catch(const std::exception&) { GotoError("invalid input", 0); }
    if(args.size() >= 5) try { ptzfSetting.tilt.speed= stoi(args[4]); } catch(const std::exception&) { GotoError("invalid input", 0); }

    err = SCRSDK::ControlPTZF(m_device_handle, (SCRSDK::CrPTZFControlType)type, &ptzfSetting);
    if(err) GotoError("", err);
Error:
    return err;
}

int presetPTZFSet(int32_t index)
{
    SCRSDK::CrError err = 0;
    err = SCRSDK::PresetPTZFSet(m_device_handle, index, SCRSDK::CrPresetPTZFSettingType_current, SCRSDK::CrPresetPTZFThumbnail_Off);
    if(err) PrintError("", err);
    return err;
}

int getLiveview(uint8_t** lv_image, CrInt32u* lv_size)
{
    int result = SCRSDK::CrError_Generic_Unknown;
    SCRSDK::CrError err = 0;
    CrInt32 num = 0;
    SCRSDK::CrLiveViewProperty* property = nullptr;
    SCRSDK::CrImageInfo imageInfo;
    SCRSDK::CrImageDataBlock image_data;
    CrInt32u bufSize = 0;
    CrInt8u* image_buff = nullptr;

    err = SCRSDK::GetLiveViewProperties(m_device_handle, &property, &num);  if(err) GotoError("", err);
    SCRSDK::ReleaseLiveViewProperties(m_device_handle, property);

    err = SCRSDK::GetLiveViewImageInfo(m_device_handle, &imageInfo);  if(err) GotoError("", err);
    bufSize = imageInfo.GetBufferSize();
    if (bufSize <= 0) GotoError("", 0);

    image_buff = new CrInt8u[bufSize];
    if (!image_buff) GotoError("", 0);

    image_data.SetData(image_buff);
    image_data.SetSize(bufSize);

    err = SCRSDK::GetLiveViewImage(m_device_handle, &image_data);  if(err) GotoError("", err);
    if (image_data.GetSize() <= 0) GotoError("", 0);

    *lv_size = image_data.GetImageSize();
    *lv_image = new uint8_t[*lv_size];
    if(!*lv_image) GotoError("", 0);
    memcpy(*lv_image, image_data.GetImageData(), *lv_size);
    result = 0;
Error:
    if(image_buff) delete[] image_buff;
    return result;
}

void deleteUint8Array(uint8_t* ptr)
{
	delete[] ptr;
}
