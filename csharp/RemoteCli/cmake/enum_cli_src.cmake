## Script for enumerating RemoteCli source files
set(__cli_src_dir ${CMAKE_CURRENT_SOURCE_DIR}/app)

### Enumerate RemoteCli source files ###
message("[${PROJECT_NAME}] Indexing source files..")
set(__cli_srcs
    ${__cli_src_dir}/RemoteCli.cpp
    ${__cli_src_dir}/CrDebugString.cpp
)

## Use cli_srcs in project CMakeLists
set(cli_srcs ${__cli_srcs})
