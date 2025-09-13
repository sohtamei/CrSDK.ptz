xcopy .\RemoteCli\external\crsdk\* .\RemoteCli\Debug\ /E /Y
xcopy .\RemoteCli\Debug\* .\appPtz2\bin\x64\Debug\ /E /Y
timeout 3 > nul
