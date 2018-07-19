# mandara-wifi-auto-login-for-windows
MANDARA Wi-Fi Auto Login for Windows

This daemon program waits for network changes and sends a login request to MANDARA Wi-Fi's captive portal server.

Not well-tested. Use it at your own risk.

# Binary

http://rv-nasbi.naist.jp:8080/share.cgi?ssid=0gicMNE

# Command line options

```
-u, --user=VALUE           MANDARA username. (required)
-p, --password=VALUE       MANDARA password. (required)
-a, --adapter-name=VALUE   Wi-Fi adapter name. (default=Wi-Fi)
-c, --captive-url-pattern=VALUE
                           Regular expression pattern matching MANDARA Wi-Fi captive portal's URL.
                           (default=\Ahttps?://[^/]*?\.naist\.jp/.+)
-l, --login-url=VALUE      URL used for GET login request. {0} and {1} represent username and password, respectively.
                           (default=https://aruba.naist.jp/cgi-bin/login?cmd=authenticate&user={0}&password={1})
-t, --timeout=VALUE        HTTP request timeout in msec. (default=5000)
```

## Example

```
NetworkAutoLogin.exe -u sumi-w -p shirogane
```

# Messages

### Condition not satisfied.

Network change was detected but login request was not sent because...

 - Already connected to Internet
 - Wi-Fi is not available
 - The captive portal is unreachable
 - The captive portal is not for MANDARA Wi-Fi network

### Login successful.

Network change was detected, login request was sent and received `200 OK` response.

### Login failed.

Network change was detected, login request was sent but could not receive `200 OK` response.
