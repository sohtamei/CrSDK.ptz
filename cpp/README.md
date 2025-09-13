### sample "PTZ" on CrSDK_v2.00.00_20250805a_Win64.zip
OS : windows

### IMPORTANT NOTICE
After using this tool to operate your Sony camera, the Sony manufacturer's warranty will no longer apply.

### how to build:
```
mkdir build
cd build
cmake -A "x64" -T "v143,host=x64" ..
```
### usage:
```
usage:
   l                     - get live view
   s                     - streaming liveview
   pt <1(abs),2(rel),3(dir),4(home)> [pan] [tilt] [p-speed] [t-speed] - control ptz
   setp <1~100>          - set preset
   set <DP name> <param>
   get <DP name>
   info <DP name>
   send <command name> <param> [param]
To exit, please enter 'q'.


DP name      : FNumber for CrDeviceProperty_FNumber.
command name : Release for CrCommandId_Release.
param        : 80, 0x50 (numeric value)
```
