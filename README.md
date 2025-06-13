
### Repository link for AeroVerse XR [ AR ] Application
https://github.com/Chinmay-HS/AeroVerse-XR

### Why We Use Two Separate Repositories for VR/XR and AR

We maintain two distinct repositories for our VR/XR and AR projects because they rely on different underlying technologies and have fundamentally different interaction patterns.

Our **VR/XR project** uses **OpenXR**, which supports headset-based interactions, motion controllers, and full spatial tracking. The **AR project**, on the other hand, is built on **ARCore**, optimized for mobile platforms and real-world anchoring.

Combining both in a single codebase would create unnecessary complexity. Separate repositories let us:

- Keep build settings and SDKs optimized for each platform.
    
- Tailor UI, input systems, and model interaction logic to the environment—immersive room-scale interaction in VR versus surface-anchored content in AR.
    
- Avoid redundant dependencies that slow down development and increase app size.
    

Having dedicated repos also makes CI/CD pipelines cleaner and testing more focused per device class.

### What Our VR Experience Offers

In our VR environment, we focus on immersive, interactive learning. Users can explore complex systems like satellite payloads or jet engines from a first-person perspective, gaining insight through spatial interaction.

Key features include:

- **Exploded Views**: Users can pull apart components and inspect internal systems in 3D.
    
- **Annotations**: Each model includes floating callouts and contextual notes that always face the user.
    
- **Toolbars and UI**: We attach essential tools to the controller or anchor them in space, letting users switch between modes like annotate, rotate, or inspect.
    
- **Natural Input**: We support both controller and hand-tracking to allow intuitive interaction.
    
- **Multiplayer Support**: We're working toward shared learning rooms where users can collaborate in real time with voice and synced views.
    
- **Achievements**: We track progress using Firebase, with badges for milestones like “first disassembly” or “full component identification.”
    
- **Focused Learning**: Because the environment is fully virtual, users aren’t distracted by the real world and can focus entirely on the model in front of them.
    

This structure helps us deliver high-fidelity simulations in VR while keeping the AR counterpart lean and efficient for mobile.

### Technologies We Used & Why We Chose OpenXR Over Meta SDK

When we started building our immersive application, we made a conscious decision to use **OpenXR** as the foundation of our VR/XR workflow rather than relying solely on vendor-specific SDKs like the Meta XR SDK.

#### Why OpenXR?

OpenXR is a **cross-platform, open standard** developed by the Khronos Group that allows us to build one XR application that can run on multiple devices—**Meta Quest, HTC Vive, Windows Mixed Reality**, and eventually **Apple Vision Pro** with minimal changes to the core codebase.

We didn’t want to lock ourselves into a single ecosystem early on. While Meta’s SDK (especially via the Oculus Integration or Meta All-in-One SDK) gives rich device-specific features and better native support, it also ties the project to a **platform-dependent structure**, making it harder to scale or port in the future.

Using OpenXR gave us the flexibility to:

- Keep our **codebase modular and portable**
    
- Use **Unity’s XR Interaction Toolkit**, which is tightly integrated with OpenXR
    
- Ensure compatibility with **future XR headsets**, without rewriting input, tracking, or rendering logic
    
- Standardize interaction systems like teleportation, grab, and UI interaction across VR and MR headsets
    

#### Challenges We Faced

Initially, there was a bit of overhead in configuring OpenXR:

- We had to properly set up **interaction profiles** like “Meta Quest Touch,” “HP Reverb G2,” etc.
    
- Features like **pass-through MR** and **controller haptics** required conditional checks or custom bindings
    
- Some Meta-specific features weren’t directly accessible without fallback to Meta’s own SDK components
    

But long term, the benefits far outweighed these hurdles. OpenXR not only allowed us to future-proof the app but also prepared the ground for **AR/MR cross-compatibility**, where both ARCore and OpenXR can co-exist under the same architectural planning.

#### AR and Mobile Compatibility

On the AR side, we're using **ARCore**, which is optimized for Android-based mobile AR. This is handled in a separate repository, because mobile AR and VR/MR have significantly different rendering pipelines, interaction models, and camera controls.

#### Architecture Strategy

This dual-repo strategy—one for **VR/MR (OpenXR-based)** and one for **AR (ARCore-based)**—ensures:

- Clean separation of build targets
    
- Specialized optimizations for both environments
    
- Independent test cycles and performance tracking
    


## Project Setup Guide

This Unity project uses large external assets (3D models and videos) which are **not included in the Git repository** to avoid exceeding GitHub's file size limits. Instead, these assets are downloaded using a Python script after cloning the repository.

### Folder Structure

```
AeroVerse-XR-Headsets/
├── Assets/
│   └── Resources/
│       ├── Models/            # Folder for large 3D models (excluded from Git)
│       └── Videos/            # Folder for large video files (excluded from Git)
├── Scripts/
│   └── download_assets.py     # Script to download and extract assets
├── .github/
│   └── workflows/
│       └── build-on-release.yml  # GitHub Actions workflow for release builds
├── .gitignore
└── README.md
```

### Cloning and Setup Instructions

Follow these steps after cloning the repository:

1. Clone the repository:
    
    ```bash
    git clone https://github.com/Chinmay-HS/AeroVerse-XR-Headsets.git
    cd AeroVerse-XR-Headsets
    ```
    
2. Install Python dependencies:
    
    ```bash
    pip install gdown
    ```
    
3. Run the asset download script:
    
    ```bash
    python Scripts/download_assets.py
    ```
    

This script will:

- Download `models.zip` and `videos.zip` from Google Drive.
    
- Extract them to `Assets/Resources/Models` and `Assets/Resources/Videos`.
    
- Remove the zip files after extraction.
    

### Git Ignore Strategy

Large asset folders are excluded via `.gitignore`:

```gitignore
Assets/Resources/Models/*
Assets/Resources/Videos/*
!Assets/Resources/Models/.gitkeep
!Assets/Resources/Videos/.gitkeep
```

Empty `.gitkeep` files are committed to ensure the folders exist in the repository even though the actual content is downloaded externally.

### Unity Editor Setup

1. Open the project using Unity Hub.
    
2. Let Unity reimport all assets.
    
3. Ensure that the `Assets/Resources/Models` and `Assets/Resources/Videos` folders are populated after running the download script.
    
4. You can now run and build the project normally.
    

---

## GitHub Actions CI/CD (Release Builds Only)

This project includes a GitHub Actions workflow that:

- Runs only when a new GitHub Release is published.
    
- Downloads external assets using the Python script.
    
- Builds an Android APK using Unity.
    
- Uploads the APK to the GitHub Release page.
    

### Workflow File

The workflow is located at:

```
.github/workflows/build-on-release.yml
```

To trigger the workflow:

1. Go to your GitHub repository.
    
2. Navigate to the "Releases" tab.
    
3. Click "Draft a new release".
    
4. Fill in version information and click "Publish release".
    

This will automatically:

- Clone the repo on GitHub's runner
    
- Download models/videos using the Python script
    
- Build the APK using Unity in headless mode
    
- Upload the APK as a release asset
    






