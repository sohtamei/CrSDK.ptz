// "get live view with http and ptz" sample
#include "httplib.h"

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
  #define CrCout std::wcout
  #define DELIMITER CRSTR("\\")
#else
  using CrString = std::string;
  #define CRSTR(s) s
  #define CrCout std::cout
  #define DELIMITER CRSTR("/")
#endif


#include "CRSDK/CrDeviceProperty.h"
#include "CRSDK/CameraRemote_SDK.h"
#include "CRSDK/IDeviceCallback.h"
#include "CrDebugString.h"   // use CrDebugString.cpp

#define PrintError(msg, err) { fprintf(stderr, "Error in %s(%d):" msg ",%s\n", __FUNCTION__, __LINE__, (err ? CrErrorString(err).c_str():"")); }
#define GotoError(msg, err) { PrintError(msg, err); goto Error; }

bool  m_connected = false;
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

std::promise<void>* m_lvPromise = nullptr;
std::mutex m_lvPromiseMutex;
void setLvPromise(std::promise<void>* dp)
{
    std::lock_guard<std::mutex> lock(m_lvPromiseMutex);
    m_lvPromise = dp;
}

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
    if (devProp.GetValueType() == SCRSDK::CrDataType_STR) GotoError("STR is not supported", 0);
    if (blocking && devProp.GetCurrentValue() == data) {
        std::cout << "skipped\n";
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
    std::cout << "OK\n";

    result = 0;
Error:
    setEventPromise(nullptr);
    return result;
}

std::vector<int64_t> _getPossible(SCRSDK::CrDeviceProperty* devProp)
{
/*
    CrInt32u GetSetValueSize();
    CrInt8u* GetSetValues();
*/
    SCRSDK::CrDataType dataType = devProp->GetValueType();
    std::vector<int64_t> possible;

    int dataLen = 1;
    switch(dataType & 0x100F) {
    case SCRSDK::CrDataType_UInt8:  dataLen = sizeof(uint8_t); break;
    case SCRSDK::CrDataType_Int8:   dataLen = sizeof(int8_t); break;
    case SCRSDK::CrDataType_UInt16: dataLen = sizeof(uint16_t); break;
    case SCRSDK::CrDataType_Int16:  dataLen = sizeof(int16_t); break;
    case SCRSDK::CrDataType_UInt32: dataLen = sizeof(uint32_t); break;
    case SCRSDK::CrDataType_Int32:  dataLen = sizeof(int32_t); break;
    case SCRSDK::CrDataType_UInt64: dataLen = sizeof(uint64_t); break;
    default: return possible;
    }

    unsigned char const* buf = devProp->GetValues();
    uint32_t nval = devProp->GetValueSize() / dataLen;
    possible.resize(nval);
    for (uint32_t i = 0; i < nval; ++i) {
        int64_t data = 0;
        switch(dataType & 0x100F) {
        case SCRSDK::CrDataType_UInt8:  data = (reinterpret_cast<uint8_t const*>(buf))[i]; break;
        case SCRSDK::CrDataType_Int8:   data = (reinterpret_cast<int8_t const*>(buf))[i]; break;
        case SCRSDK::CrDataType_UInt16: data = (reinterpret_cast<uint16_t const*>(buf))[i]; break;
        case SCRSDK::CrDataType_Int16:  data = (reinterpret_cast<int16_t const*>(buf))[i]; break;
        case SCRSDK::CrDataType_UInt32: data = (reinterpret_cast<uint32_t const*>(buf))[i]; break;
        case SCRSDK::CrDataType_Int32:  data = (reinterpret_cast<int32_t const*>(buf))[i]; break;
        case SCRSDK::CrDataType_UInt64: data = (reinterpret_cast<uint64_t const*>(buf))[i]; break;
        default: break;
        }
        possible.at(i) = data;
    }
    return possible;
}

class DeviceCallback : public SCRSDK::IDeviceCallback
{
public:
    DeviceCallback() {};
    ~DeviceCallback() {};

    void OnConnected(SCRSDK::DeviceConnectionVersioin version)
    {
        std::cout << "Connected to " << m_modelId << "\n";
        m_connected = true;
        std::lock_guard<std::mutex> lock(m_eventPromiseMutex);
        if(m_eventPromise) {
            m_eventPromise->set_value();
            m_eventPromise = nullptr;
        }
    }

    void OnError(CrInt32u error)
    {
        printf("Connection error:%s\n", CrErrorString(error).c_str());
        std::lock_guard<std::mutex> lock(m_eventPromiseMutex);
        if(m_eventPromise) {
            m_eventPromise->set_exception(std::make_exception_ptr(std::runtime_error("error")));
            m_eventPromise = nullptr;
        }
    }

    void OnDisconnected(CrInt32u error)
    {
        std::cout << "Disconnected from " << m_modelId << "\n";
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
        std::cout << "OnNotifyContentsTransfer.\n";
    }

    void OnWarning(CrInt32u warning)
    {
        if (warning == SCRSDK::CrWarning_Connect_Reconnecting) {
            std::cout << "Reconnecting to " << m_modelId << "\n";
            return;
        }
    }

    void OnWarningExt(CrInt32u warning, CrInt32 param1, CrInt32 param2, CrInt32 param3) {}
    void OnLvPropertyChanged() {}
    void OnLvPropertyChangedCodes(CrInt32u num, CrInt32u* codes) {}
    void OnPropertyChanged() {}
    void OnPropertyChangedCodes(CrInt32u num, CrInt32u* codes)
    {
        //std::cout << "OnPropertyChangedCodes:\n";
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
                CrCout << "  " << CrString(name.begin(), name.end()) << "=\"" << _getCurrentStr(&devProp) << "\"\n";
            } else {
                int64_t current = devProp.GetCurrentValue();
                if(current < 10) {
                    printf("  %s=%" PRId64 "\n", name.c_str(), current);
                } else {
                    printf("  %s=0x%" PRIx64 "(%" PRId64 ")\n", name.c_str(), current, current);
                }
            }
*/
        }
    }
    void OnNotifyMonitorUpdated(CrInt32u type, CrInt32u frameNo)
    {
        if(type == SCRSDK::CrMonitorUpdated_LiveView) {
    //  printf("%x", frameNo & 0xF);
            std::lock_guard<std::mutex> lock(m_lvPromiseMutex);
            if(m_lvPromise) {
                m_lvPromise->set_value();
                m_lvPromise = nullptr;
            }
        }
    }

};

SCRSDK::CrError _getLiveView(int64_t device_handle, CrString path)
{
    int result = SCRSDK::CrError_Generic_Unknown;
    SCRSDK::CrError err = 0;
    CrInt32 num = 0;
    SCRSDK::CrLiveViewProperty* property = nullptr;
    SCRSDK::CrImageInfo imageInfo;
    SCRSDK::CrImageDataBlock image_data;
    CrInt32u bufSize = 0;
    CrInt8u* image_buff = nullptr;

    err = SCRSDK::GetLiveViewProperties(device_handle, &property, &num);  if(err) GotoError("", err);
    SCRSDK::ReleaseLiveViewProperties(device_handle, property);

    err = SCRSDK::GetLiveViewImageInfo(device_handle, &imageInfo);  if(err) GotoError("", err);
    bufSize = imageInfo.GetBufferSize();
    if (bufSize <= 0) GotoError("", 0);

    image_buff = new CrInt8u[bufSize];
    if (!image_buff) GotoError("", 0);

    image_data.SetData(image_buff);
    image_data.SetSize(bufSize);

    err = SCRSDK::GetLiveViewImage(device_handle, &image_data);  if(err) GotoError("", err);
    if (image_data.GetSize() <= 0) GotoError("", 0);

    {
        path.append(DELIMITER CRSTR("LiveView000000.JPG"));
        std::ofstream file(path, std::ios::out | std::ios::binary);
        if (file.bad()) GotoError("", 0);
        file.write((char*)image_data.GetImageData(), image_data.GetImageSize());
        file.close();
        CrCout << path.data() << '\n';
    }
    result = 0;
Error:
    if(image_buff) delete[] image_buff;
    return result;
}

//-------------------------------

#include <atomic>

std::atomic<bool> running(true);

SCRSDK::CrError _getLiveView2(int64_t device_handle, CrInt8u** lv_image, CrInt32u* lv_size)
{
    int result = SCRSDK::CrError_Generic_Unknown;
    SCRSDK::CrError err = 0;
    CrInt32 num = 0;
    SCRSDK::CrLiveViewProperty* property = nullptr;
    SCRSDK::CrImageInfo imageInfo;
    SCRSDK::CrImageDataBlock image_data;
    CrInt32u bufSize = 0;
    CrInt8u* image_buff = nullptr;

    err = SCRSDK::GetLiveViewProperties(device_handle, &property, &num);  if(err) GotoError("", err);
    SCRSDK::ReleaseLiveViewProperties(device_handle, property);

    err = SCRSDK::GetLiveViewImageInfo(device_handle, &imageInfo);  if(err) GotoError("", err);
    bufSize = imageInfo.GetBufferSize();
    if (bufSize <= 0) GotoError("", 0);

    image_buff = new CrInt8u[bufSize];
    if (!image_buff) GotoError("", 0);

    image_data.SetData(image_buff);
    image_data.SetSize(bufSize);

    err = SCRSDK::GetLiveViewImage(device_handle, &image_data);  if(err) GotoError("", err);
    if (image_data.GetSize() <= 0) GotoError("", 0);

    *lv_size = image_data.GetImageSize();
    *lv_image = new CrInt8u[*lv_size];
    if(!*lv_image) GotoError("", 0);
    memcpy(*lv_image, image_data.GetImageData(), *lv_size);
    result = 0;
Error:
    if(image_buff) delete[] image_buff;
    return result;
}

bool streamLiveview(size_t offset, httplib::DataSink &sink)
{
    bool result = false;
    SCRSDK::CrError err = 0;
    CrInt8u* image_buff = nullptr;
    CrInt32u bufSize = 0;

    std::promise<void> lvPromise;
    std::future<void> lvFuture = lvPromise.get_future();
    std::future_status status;

    setLvPromise(&lvPromise);
    status = lvFuture.wait_for(std::chrono::milliseconds(3000));
    if(status != std::future_status::ready) GotoError("timeout", 0);
    try{
        lvFuture.get();
    } catch(const std::exception&) GotoError("", 0);

    err = _getLiveView2(m_device_handle, &image_buff, &bufSize);
    if(err) return false;

    {
        char buf[256] = {0};
        int len = snprintf(buf, sizeof(buf), "--frame\r\n"
                                            "Content-Type: image/jpeg\r\n"
                                            "Content-Length: %d\r\n\r\n", bufSize);
        if(len >= sizeof(buf)) GotoError("", 0);
        sink.write(buf, len);
    }
    sink.write((char*)image_buff, bufSize);
    sink.write("\r\n", 2);

    //std::this_thread::sleep_for(std::chrono::milliseconds(33));
    result = true;
Error:
    setLvPromise(nullptr);
    delete[] image_buff;
    image_buff = nullptr;
    return result;
}

void handle_request(const httplib::Request& req, httplib::Response& res)
{
    res.set_header("Access-Control-Allow-Origin", "*");
    res.set_chunked_content_provider(
        "multipart/x-mixed-replace; boundary=frame", streamLiveview
    );
}

void server_thread(httplib::Server& svr)
{
    std::cout << "please access to http://localhost:8080\n";
    svr.Get("/", handle_request);
    svr.listen("0.0.0.0", 8080);
    running = false;
}

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

int64_t _stoll(std::string inputLine)
{
    int64_t data = 0;
    if(inputLine.empty()) throw std::runtime_error("error");
    try {
        if (inputLine.compare(0, 2, "0x") == 0 || inputLine.compare(0, 2, "0X") == 0) {
            data = std::stoull(inputLine.substr(2), nullptr, 16);
        } else {
            data = std::stoll(inputLine, nullptr, 10);
        }
    } catch(const std::exception& ex) {
        throw ex;
    }
    return data;
}

int main(void)
{
    int result = -1;
    SCRSDK::CrError err = SCRSDK::CrError_None;
    SCRSDK::ICrEnumCameraObjectInfo* enumCameraObjectInfo = nullptr;
    SCRSDK::ICrCameraObjectInfo* objInfo = nullptr;
    DeviceCallback deviceCallback;
    httplib::Server svr;
    std::thread* serverThread = nullptr;

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
        std::string inputLine;
        uint32_t model = SCRSDK::CrCameraDeviceModel_BRC_AM7;
        std::string  userId = "";
        std::string  userPassword = "";
        CrInt8u macAddress[6] = {0};
        CrInt32u ipAddress = 0;
        bool SSHsupport = false;

        std::cout << "usage:<ipaddress> [userid] [pass]\n";
        std::getline(std::cin, inputLine);
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
                std::cout << "fingerprint: " << fpBuff << "\n";
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
    if(err) goto Error;

    std::cout << "usage:\n";
//  std::cout << "   p <1(Main),2(httpLV)> - set live view protocol\n";
    std::cout << "   l                     - get live view\n";
    std::cout << "   s                     - streaming liveview \n";
    std::cout << "   pt <1(abs),2(rel),3(dir),4(home)> [pan] [tilt] [p-speed] [t-speed] - control ptz \n";
    std::cout << "   setp <1~100>          - set preset\n";
    std::cout << "   set <DP name> <param>\n";
    std::cout << "   get <DP name>\n";
    std::cout << "   info <DP name>\n";
    std::cout << "   send <command name> <param> [param]\n";
    std::cout << "To exit, please enter 'q'.\n";

    while(1) {
        std::string inputLine;
        std::getline(std::cin, inputLine);
        std::vector<std::string> args = _split(inputLine, ' ');

        if(args.size() == 0) {
/*
        } else if(args[0] == "p" && args.size() >=2) {
            uint32_t val;
            try { val = stoi(args[1]); } catch(const std::exception&) { GotoError("", 0); }
            err = _setDeviceProperty(m_device_handle, SCRSDK::CrDeviceProperty_LiveViewProtocol, val);
            if(err) goto Error;
*/
        } else if(args[0] == "l" || args[0] == "L") {
            err = _getLiveView(m_device_handle, path);
            if(err) goto Error;

        } else if(args[0] == "s" || args[0] == "S") {
            if(!serverThread) {
                serverThread = new std::thread(server_thread, std::ref(svr));
            }
        } else if(args[0] == "pt" && args.size() >= 2) {
            #define SPEED_MAX 50 // 127

            uint32_t type = 0;
            SCRSDK::CrPTZFSetting ptzfSetting;
            ptzfSetting.pan.exists = 1;
            ptzfSetting.pan.position = 0;
            ptzfSetting.pan.speed = SPEED_MAX;
            ptzfSetting.tilt.exists = 1;
            ptzfSetting.tilt.position = 0;
            ptzfSetting.tilt.speed = SPEED_MAX;

            try { type = stoi(args[1]); } catch(const std::exception&) { GotoError("", 0); }
            if(args.size() >= 3) try { ptzfSetting.pan.position = (int)_stoll(args[2]); } catch(const std::exception&) { GotoError("invalid input", 0); }
            if(args.size() >= 4) try { ptzfSetting.tilt.position = (int)_stoll(args[3]); } catch(const std::exception&) { GotoError("invalid input", 0); }
            if(args.size() >= 5) try { ptzfSetting.pan.speed= (int)_stoll(args[4]); } catch(const std::exception&) { GotoError("invalid input", 0); }
            if(args.size() >= 6) try { ptzfSetting.tilt.speed= (int)_stoll(args[5]); } catch(const std::exception&) { GotoError("invalid input", 0); }

            err = SCRSDK::ControlPTZF(m_device_handle, (SCRSDK::CrPTZFControlType)type, &ptzfSetting);
            if(err) GotoError("", err);

        } else if(args[0] == "setp" && args.size() >= 2) {
            int index = 0;
            try { index = stoi(args[1]); } catch(const std::exception&) { GotoError("", 0); }
            err = SCRSDK::PresetPTZFSet(m_device_handle, index, SCRSDK::CrPresetPTZFSettingType_current, SCRSDK::CrPresetPTZFThumbnail_Off);
            if(err) GotoError("", err);
            std::cout << "OK\n";

        } else if(args[0] == "q" || args[0] == "Q") {
            break;
        } else if(args[0] == "send" && args.size() >= 3) {
            int64_t data = 0;
            int32_t code = CrCommandIdCode(args[1]);
            if(code < 0) continue;
            try{ data = _stoll(args[2]); } catch(const std::exception&) {continue;}

            err = SCRSDK::SendCommand(m_device_handle, code, (SCRSDK::CrCommandParam)data);
            if(err) GotoError("", err);

		    if(args.size() >= 4) {
			    std::this_thread::sleep_for(std::chrono::milliseconds(50));
				try{ data = _stoll(args[3]); } catch(const std::exception&) GotoError("", 0);

			    err = SCRSDK::SendCommand(m_device_handle, code, (SCRSDK::CrCommandParam)data);
			    if(err) GotoError("", err);
		    }

        } else if(args[0] == "set" && args.size() >= 3) {
            int64_t data = 0;
            int32_t code = CrDevicePropertyCode(args[1]);
            if(code < 0) continue;

            try{ data = _stoll(args[2]); } catch(const std::exception&) {continue;}

            err = _setDeviceProperty(m_device_handle, code, data, false/*blocking*/);
            if(err) continue;

        } else if((args[0] == "get" || args[0] == "info") && args.size() >= 2) {
        // device property get/info
            int32_t code = CrDevicePropertyCode(args[1]);
            if(code < 0) continue;

            SCRSDK::CrDeviceProperty devProp;
            err = _getDeviceProperty(m_device_handle, code, &devProp);
            if(err) continue;
            SCRSDK::CrDataType dataType = devProp.GetValueType();

            if(args[0] == "get") {
                if(dataType == SCRSDK::CrDataType_STR) {
                    //CrCout << _getCurrentStr(&devProp) << "\n";
                } else {
                    printf("0x%" PRIx64 "(%" PRId64 ")\n", devProp.GetCurrentValue(), devProp.GetCurrentValue());  // macro for %lld
                }
            } else if(args[0] == "info") {
                printf("  get enable=%d\n", devProp.IsGetEnableCurrentValue());
                printf("  set enable=%d\n", devProp.IsSetEnableCurrentValue());
                printf("  variable  =%d\n", devProp.GetPropertyVariableFlag());
                printf("  enable    =%d\n", devProp.GetPropertyEnableFlag());
                printf("  valueType =0x%x\n", dataType);
                if(dataType == SCRSDK::CrDataType_STR) {
                    //CrCout << "  current   =\"" << _getCurrentStr(&devProp) << "\"\n";
                } else {
                    printf("  current   =0x%" PRIx64 "(%" PRId64 ")\n", devProp.GetCurrentValue(), devProp.GetCurrentValue());

                    std::vector<int64_t> possible = _getPossible(&devProp);
                    printf("  possible  =");
                    for(int i = 0; i < possible.size(); i++) {
                        printf("0x%" PRIx64 "(%" PRId64 "),", possible[i], possible[i]);
                    }
                    printf("\n");
                }
            }
        } else {
            std::cout << "unknown command\n";
        }
    }

    result = 0;
Error:
    if(serverThread) {
        svr.stop();
        while(running);
        serverThread->join();
    }
    if(enumCameraObjectInfo) enumCameraObjectInfo->Release();

    if(m_connected) {
        std::promise<void> eventPromise;
        std::future<void> eventFuture = eventPromise.get_future();
        setEventPromise(&eventPromise);
        SCRSDK::Disconnect(m_device_handle);
        eventFuture.wait_for(std::chrono::milliseconds(3000));
    }
    if(m_device_handle) SCRSDK::ReleaseDevice(m_device_handle);
    SCRSDK::Release();

    return result;
}
