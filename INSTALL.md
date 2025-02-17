# Install Instructions
1. Download and install Raspberry Pi Imager: https://www.raspberrypi.com/software/
2. Connect your SD card to a PC and use the following options in Raspberry Pi Imager:
    - Raspberry Pi Device: Raspberry Pi Zero 2 W
    - Operating System: Other -> Raspberry Pi OS Lite (64-bit)
    - Storage: (select your SD card)
    - Click "Next"
    - When asked, click "Edit Settings" for Use OS customisation?
    - Set hostname: "jrencoder"
    - Set username and password: (your choice for both of these)
    - Configure wireless LAN: (enter your Wi-Fi SSID, password, and country)
    - Set locale settings: (select your timezone)
    - Under the services tab, select "Enable SSH"
    - If you know how to use SSH keys, paste in your public key instead of using password authentication
    - Click "Save"
    - Click "Yes" for "Would you like to apply OS customisation settings?"
    - Click "Yes" to continue imaging
3. Insert the SD card into the Pi
4. Connect either the USB-C port to a charger, or the barrel jack to a 5V power supply
5. Connect to the Pi over SSH. Open a terminal program on your PC and run this command:
   - `ssh [your username]@jrencoder.local`
   - Replace [your username] with what you chose during imaging. If this doesn't work you may need to find the Pi's IP and use that instead of the hostname.
6. Run this command over SSH to install this software:
   - `curl -sSL https://raw.githubusercontent.com/Jessecar96/wsjr-data-encoder/refs/heads/main/install.sh | bash`
7. Once finished, edit your config.json file using `nano $HOME/jrencoder/config.json`
   - Obtain a weather.com API key and paste it into the "apikey" value
   - Under "stars" fill in your star's switches, location, and location name.
8. Reboot the pi using this command: `sudo reboot now`
9. When rebooted, you should see your star showing data!

## How to find your star's switches
### The Weather Star Jr
1. Connect an AT or PS/2 keyboard (with adapter) to the front of the star
2. Press Alt + F10
3. The hexadecimal value after "SW" are your switches. They are 8 characters long.
### The Weather Star
1. Remove the front panel
2. Read the rotary switches inside from left to right, there are 8 of them 