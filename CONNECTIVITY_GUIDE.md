# Connectivity Guide

This guide helps you connect your phone, tablet, or other computers to the QuickTextTransporter Web Server.

## 1. Local Network Connection (Wi-Fi)

The easiest way to connect is when both devices are on the same Wi-Fi network.

1.  **Start the Web Server**:
    - Open QuickTextTransporter on your PC.
    - Click **Enable Web Server** in the "Web Server" dropdown.
    - The status text will change to something like `http://192.168.1.5:45680`.

2.  **Connect from Mobile**:
    - Ensure your phone is connected to the **same Wi-Fi network** as your PC.
    - Open your phone's web browser (Chrome, Safari, etc.).
    - Type the address exactly as shown in the app (e.g., `http://192.168.1.5:45680`).

3.  **Troubleshooting**:
    - **"Site can't be reached"**:
        - Check if your PC's Firewall is blocking the connection. The app attempts to add rules automatically, but you might need to allow "QuickTextTransporter" manually in Windows Defender Firewall.
        - Ensure both devices are on the same network (e.g., not one on Guest Wi-Fi and one on Main).

## 2. Internet Connection (Advanced)

If you want to connect from outside your home network (e.g., using mobile data), you need to expose your local server to the internet.

### Option A: Cloudflare Tunnel (Recommended)
This is the safest and easiest method. It gives you a secure `https://...` link.

1.  Download `cloudflared` from Cloudflare.
2.  Run the following command in your terminal:
    ```powershell
    cloudflared tunnel --url http://localhost:45680
    ```
3.  It will give you a temporary URL (e.g., `https://random-name.trycloudflare.com`).
4.  Open that URL on any device to connect.

### Option B: Ngrok
Similar to Cloudflare Tunnel.

1.  Download `ngrok`.
2.  Run:
    ```powershell
    ngrok http 45680
    ```
3.  Use the provided `https://...` URL.

### Option C: Port Forwarding
**Warning**: This exposes your PC directly to the internet. Not recommended unless you know what you are doing.

1.  Log in to your router settings.
2.  Forward TCP port `45680` to your PC's local IP address.
3.  Find your Public IP (search "what is my ip" on Google).
4.  Connect using `http://YOUR_PUBLIC_IP:45680`.
