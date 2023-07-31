---
order: 3
icon: container
---
# Installing and Setting Up SQL Server 2019 in Docker

+++ macOS
This is a Tab
+++ Linux
This is a Tab
+++ Windows

## WSL 2 backend

- WSL version 1.1.3.0 or above.
- Windows 11 64-bit: Home or Pro version 21H2 or higher, or Enterprise or Education version 21H2 or higher.
- Windows 10 64-bit: Home or Pro 21H2 (build 19044) or higher, or Enterprise or Education 21H2 (build 19044) or higher.
- Enable the WSL 2 feature on Windows. For detailed instructions, refer to the [Microsoft documentation](https://docs.microsoft.com/en-us/windows/wsl/install-win10).
- The following hardware prerequisites are required to successfully run WSL 2 on Windows 10 or Windows 11:
  - 64-bit processor with [Second Level Address Translation (SLAT)](https://en.wikipedia.org/wiki/Second_Level_Address_Translation)
  - 4GB system RAM
  - BIOS-level hardware virtualization support must be enabled in the BIOS settings. For more information, see [Virtualization](https://docs.docker.com/desktop/troubleshoot/topics/#virtualization).
- Download and install the [Linux kernel update package](https://docs.microsoft.com/windows/wsl/wsl2-kernel).

## Install Docker and Docker Desktop

1. Download Docker Desktop for Windows from https://www.docker.com/products/docker-desktop.
2. Open the downloaded setup and grant administrator privileges, if required.
3. Follow the setup wizard to complete the installation of Docker Desktop.
4. Restart your PC for the changes to take effect.
5. Start Docker Desktop from the Windows Start menu, then select the Docker icon from the hidden icons menu of your taskbar.

## Docker Containers

- (x86)

``` output
docker pull mcr.microsoft.com/mssql/server
```
- (arm) 
``` output
docker pull mcr.microsoft.com/mssql/server
```
+++