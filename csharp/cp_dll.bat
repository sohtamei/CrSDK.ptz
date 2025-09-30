xcopy .\RemoteCli\external\crsdk\* .\RemoteCli\Debug\ /E /Y
xcopy .\RemoteCli\external\crsdk\* .\RemoteCli\Release\ /E /Y
xcopy .\RemoteCli\Debug\* .\appPtz2\bin\x64\Debug\ /E /Y
xcopy .\RemoteCli\Release\* .\appPtz2\bin\x64\Release\ /E /Y
timeout 3 > nul
