
#include <SDKDDKVer.h>

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
#include <windows.h>
#include <memory>
#define RemoteCli_API __declspec(dllexport)


extern "C" __declspec(dllexport)
int RemoteCli_connect(char* inputLine); //<ipaddress> [userid] [pass]

extern "C" __declspec(dllexport)
int RemoteCli_disconnect(void);

extern "C" __declspec(dllexport)
int setDeviceProperty(char* code, int64_t data, bool blocking);

extern "C" __declspec(dllexport)
int sendCommand(char* inputLine);

extern "C" __declspec(dllexport)
int controlPTZF(char* type);

extern "C" __declspec(dllexport)
int presetPTZFSet(int32_t index);

extern "C" __declspec(dllexport)
int getLiveview(uint8_t** lv_image, CrInt32u* lv_size);

extern "C" __declspec(dllexport)
void deleteUint8Array(uint8_t* ptr);

typedef void (*LiveviewCbFunc)(int eventId);

extern "C" __declspec(dllexport)
void RegisterLiveviewCb(LiveviewCbFunc liveviewCb);
