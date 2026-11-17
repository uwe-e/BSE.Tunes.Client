# Sideload line of business apps

## Step 1: Turn on sideloading

### Windows 11 setting

1. Open the Settings app
2. Go to System and select **Erweitert - Leistungsoptimierung und Entwicklerfeatures**
3. Turn on the **Entwicklermodus**
![Entwicklermodus](Developer-mode.png)

## Step 2: Import the security certificate

This step installs the app certificate to the local device. Installing the certificate creates the trust between the app and the device.

1. Download the app package
2. Open the Properties for the app package.
![Open Properties](open-properties.png)
  - Go to the **Digital Signaturen** tab.
  - Select the certificate, and select **Details** to open the digital signature details window.
  - Select **Zertifikat anzeigen** to open the certificate window.
  ![show certificate](show-cert.png)
  - Select **Zertifikat installieren** to launch the certificate import wizard.
  ![install certificate](install-cert.png)

  3. On the **Zertifikatimport-Assistenten**, select Local Machine. This action might require an administrator to elevate.
  ![install on local machine](select-local-machine.png)
  4. Continue the process to import the certificate into the Trusted Root Certification Authorities store.
  ![trusted root certification](trusted-root-crt.png)

 ## Step 3: Install the app

 After you enable sideloading and import the certificate, there are multiple methods you can use to install the app on devices.

 1. Manually open the .msix or .appx package in Windows Explorer.