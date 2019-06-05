FROM mono:5.20
COPY ./Server /usr/src/Server
COPY ./RPC /usr/src/RPC
WORKDIR /usr/src/Server
RUN msbuild /p:Configuration=Release /p:Platform=x86
EXPOSE 5678
ENTRYPOINT mono ./bin/Release/Server.exe
