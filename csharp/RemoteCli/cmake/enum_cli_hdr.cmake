## Script for enumerating RemoteCli header files
set(__cli_hdr_dir ${CMAKE_CURRENT_SOURCE_DIR}/app)

### Enumerate RemoteCli header files ###
message("[${PROJECT_NAME}] Indexing header files..")
set(__cli_hdrs
)

## Use cli_srcs in project CMakeLists
set(cli_hdrs ${__cli_hdrs})
