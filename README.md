HALLLLLLLLLLL
===============

HAL専科用のサンプルコード

## Clone

```
git clone --recursive https://github.com/aiming/senka-hal-tokyo.git
```

## Build

* クライアント
  * Unityで実行

* サーバー
  - Debug
  ```
  xbuild Server/Server.sln
  ```
  - Release
  ```
  xbuild /p:Configuration=Release Server/Server.sln
  ```

## Run

* クライアント
  * Unityで実行

* サーバー
  - Debug
  ```
  mono Server/bin/Debug/Server.exe
  ```
  - Release
  ```
  mono Server/bin/Release/Server.exe
  ```

## Copyright

* websocket-sharp  
  Copyright (c) 2010-2017 sta.blockhead  
  https://github.com/sta/websocket-sharp/blob/master/LICENSE.txt

* Newtonsoft.Json  
  Copyright (c) 2007 James Newton-King  
  https://github.com/JamesNK/Newtonsoft.Json/blob/master/LICENSE.md

## Original
https://github.com/yukinarit/WebSocketSample by [yukinarit](https://github.com/yukinarit/)
