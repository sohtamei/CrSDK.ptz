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

int currentIndex = 0;

#define PrintError(msg, err) { fprintf(stderr, "Error in %s(%d):" msg ",%s\n", __FUNCTION__, __LINE__, (err ? CrErrorString(err).c_str():"")); }
#define GotoError(msg, err) { PrintError(msg, err); goto Error; }

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


class CameraDevice : public SCRSDK::IDeviceCallback
{
public:
	int64_t  m_device_handle = 0;
	bool  m_connected = false;
	bool  m_disconnect_req = false;
	std::string m_modelId;
	int m_index = 0;

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

	SCRSDK::CrError getDeviceProperty(uint32_t code, SCRSDK::CrDeviceProperty* devProp)
	{
	    std::int32_t nprop = 0;
	    SCRSDK::CrDeviceProperty* prop_list = nullptr;
	    SCRSDK::CrError err = SCRSDK::GetSelectDeviceProperties(m_device_handle, 1, &code, &prop_list, &nprop);
	    if(err) GotoError("", err);
	    if(prop_list && nprop >= 1) {
	        *devProp = prop_list[0];
	    }
	Error:
	    if(prop_list) SCRSDK::ReleaseDeviceProperties(m_device_handle, prop_list);
	    return err;
	}

	SCRSDK::CrError setDeviceProperty(uint32_t code, uint64_t data, bool blocking=true)
	{
	    int result = SCRSDK::CrError_Generic_Unknown;
	    SCRSDK::CrError err = 0;
	    std::promise<void> eventPromise;
	    std::future<void> eventFuture = eventPromise.get_future();
	    std::future_status status;

	    SCRSDK::CrDeviceProperty devProp;

	    err = getDeviceProperty(code, &devProp);
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
	    err = SCRSDK::SetDeviceProperty(m_device_handle, &devProp);
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
        }
        if(m_liveviewCb && !m_disconnect_req && m_index == currentIndex)
            m_liveviewCb(1, m_index);
    }
    
    void OnNotifyMonitorUpdated(CrInt32u type, CrInt32u frameNo)
    {
        if(type == SCRSDK::CrMonitorUpdated_LiveView) {
    //  fprintf(stderr, "%x", frameNo & 0xF);
            if(m_liveviewCb && !m_disconnect_req && m_index == currentIndex)
		        m_liveviewCb(0, m_index);
        }
    }

    SCRSDK::CrError connect(std::string modelId,
			SCRSDK::ICrCameraObjectInfo* objInfo,
		    std::string  userId,
		    std::string  userPassword)
    {
		SCRSDK::CrError err;
	  #if defined(__APPLE__)
	    #define MAC_MAX_PATH 255
	    char pathBuf[MAC_MAX_PATH] = {0};
	    if(NULL == getcwd(pathBuf, sizeof(pathBuf) - 1)) return 1;
	    CrString path = pathBuf;
	  #else
	    CrString path = fs::current_path().native();
	  #endif
	    m_modelId = modelId;

	    // connect
	    {
	        char fpBuff[128] = {0};
	        CrInt32u fpLen = 0;
	        std::promise<void> eventPromise;
	        std::future<void> eventFuture = eventPromise.get_future();

	        if (objInfo->GetSSHsupport() == SCRSDK::CrSSHsupport_ON) {
	            err = SCRSDK::GetFingerprint(objInfo, fpBuff, &fpLen);
	            if(err) GotoError("", err);
	            std::cerr << "fingerprint: " << fpBuff << "\n";
	        }

	        setEventPromise(&eventPromise);
	        err = SCRSDK::Connect(objInfo, this, &m_device_handle,
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

	    // set work directory
	    {
	        CrCout << "path=" << path.data() << "\n";
	        err = SCRSDK::SetSaveInfo(m_device_handle, const_cast<CrChar*>(path.data()), const_cast<CrChar*>(CRSTR("DSC")), -1/*startNo*/);
	        if(err) GotoError("", err);
	    }

	    std::this_thread::sleep_for(std::chrono::milliseconds(1000));

	    // set LiveViewProtocol=2(http)
	    if(m_index == 0) {
		    err = setDeviceProperty(SCRSDK::CrDeviceProperty_LiveViewProtocol, 2/*http*/);
		    //if(err) goto Error;
		}
	    return 0;
	Error:
	    disconnect();
    	return -1;
	}

    SCRSDK::CrError disconnect(void)
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

	    m_disconnect_req = false;
	    m_connected = false;
	    m_device_handle = 0;
	    fprintf(stderr, "xxx\n");
	    return 0;
	}
};


CameraDevice cameraDevice[3];

void RegisterLiveviewCb(LiveviewCbFunc liveviewCb)
{
	for(int i = 0; i < 3; i++) {
		cameraDevice[i].RegisterLiveviewCb(liveviewCb);
		cameraDevice[i].m_index = i;
	}
}

int RemoteCli_init(void)
{
    bool boolRet = SCRSDK::Init();
    if(!boolRet) GotoError("", 0);
    return 0;
Error:
	return -1;
}

int RemoteCli_Release(void)
{
    SCRSDK::Release();
    return 0;
}

int RemoteCli_connect(int index, char* inputLine)
{
    SCRSDK::CrError err = SCRSDK::CrError_None;
    SCRSDK::ICrCameraObjectInfo* objInfo = nullptr;

    uint32_t model = SCRSDK::CrCameraDeviceModel_BRC_AM7;
    std::string  userId = "";
    std::string  userPassword = "";
    CrInt8u macAddress[6] = {index,index,index,index,index,index};
    CrInt32u ipAddress = 0;
    bool SSHsupport = false;

	std::vector<std::string> ips;
    std::vector<std::string> args = _split(inputLine, ' ');
    if(args.size() < 1) GotoError("invalid input", 0);

    ips = _split(args[0], '.');
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

	return cameraDevice[index].connect(args[0], (SCRSDK::ICrCameraObjectInfo*)objInfo, userId, userPassword);
Error:
    return -1;
}

int RemoteCli_disconnect(int index)
{
	return cameraDevice[index].disconnect();
}

int setCameraIndex(int index)
{
	currentIndex = index;
	return 0;
}

int setDeviceProperty(char* code, int64_t data, bool blocking=true)
{
    int32_t codeInt = CrDevicePropertyCode(code);
    if(codeInt < 0) {
        PrintError("unknown DP",0);
        return SCRSDK::CrError_Generic_Unknown;
    }
    return cameraDevice[currentIndex].setDeviceProperty(codeInt, (uint64_t)data, blocking);
}

int64_t getDeviceProperty(char* code)
{
    int32_t codeInt = CrDevicePropertyCode(code);
    if(codeInt < 0) {
        PrintError("unknown DP",0);
        return 0;
    }

    int64_t data = 0;
    SCRSDK::CrError err = 0;
    SCRSDK::CrDeviceProperty devProp;
    err = cameraDevice[currentIndex].getDeviceProperty(codeInt, &devProp);
    if(err) GotoError("", err);
    data = devProp.GetCurrentValue();
Error:
    return data;
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

int64_t incDeviceProperty(char* code, int incDec, bool blocking=true)
{
    int32_t codeInt = CrDevicePropertyCode(code);
    if(codeInt < 0) {
        PrintError("unknown DP",0);
        return 0;
    }

    int64_t data = 0;
    SCRSDK::CrError err = 0;
    SCRSDK::CrDeviceProperty devProp;

    err = cameraDevice[currentIndex].getDeviceProperty(codeInt, &devProp);
    if(err) GotoError("", err);

	if(devProp.GetPropertyEnableFlag() != 1) GotoError("not writable",0);

	{
		std::vector<int64_t> possible = _getPossible(&devProp);
		int type = (devProp.GetValueType() & 0x6000);

		data = devProp.GetCurrentValue();
		if(type == SCRSDK::CrDataType_ArrayBit) {
			for(int i = 0; i < possible.size(); i++) {
				if(data == possible[i]) {
					int index = i + incDec;
					if(index < 0) {
						index = 0;
					} else if(index > possible.size()-1) {
						index = possible.size()-1;
					}
					err = cameraDevice[currentIndex].setDeviceProperty(codeInt, possible[index], blocking);
					if(err) GotoError("", err);
					break;
				}
			}
		} else if(type == SCRSDK::CrDataType_RangeBit) {
			int64_t min = possible[0];
			int64_t max = possible[1];
			int64_t step = possible[2];
			data += incDec * step;

			if(data < min) {
				data = min;
			} else if(data > max) {
				data = max;
			}
			err = cameraDevice[currentIndex].setDeviceProperty(codeInt, data, blocking);
		    if(err) GotoError("", err);
		}
		err = cameraDevice[currentIndex].getDeviceProperty(codeInt, &devProp);
		if(err) GotoError("", err);
		data = devProp.GetCurrentValue();
	}
Error:
    return data;
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

    err = SCRSDK::SendCommand(cameraDevice[currentIndex].m_device_handle, code, (SCRSDK::CrCommandParam)data);
    if(err) GotoError("", err);

    if(args.size() >= 3) {
	    std::this_thread::sleep_for(std::chrono::milliseconds(50));
		try{ data = stoi(args[2]); } catch(const std::exception&) GotoError("", 0);

	    err = SCRSDK::SendCommand(cameraDevice[currentIndex].m_device_handle, code, (SCRSDK::CrCommandParam)data);
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

    err = SCRSDK::ControlPTZF(cameraDevice[currentIndex].m_device_handle, (SCRSDK::CrPTZFControlType)type, &ptzfSetting);
    if(err) GotoError("", err);
Error:
    return err;
}

int presetPTZFSet(int32_t preset)
{
    SCRSDK::CrError err = 0;
    err = SCRSDK::PresetPTZFSet(cameraDevice[currentIndex].m_device_handle, preset, SCRSDK::CrPresetPTZFSettingType_current, SCRSDK::CrPresetPTZFThumbnail_Off);
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
    int64_t  m_device_handle = cameraDevice[currentIndex].m_device_handle;

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
