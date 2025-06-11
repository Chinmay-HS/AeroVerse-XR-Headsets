import os
import gdown
import zipfile

# Mapping of destination folder to Google Drive file ID
files = {
    "Assets/Resources/Videos": "11dVVoE7hjNYzezV0FHkYjtFKj8koP9a2",
    "Assets/Resources/Models": "18jO-ApJXPotXaZKwS6pmFOAj1VBwkXqx"
}

for folder_path, file_id in files.items():
    folder_name = os.path.basename(folder_path)
    zip_name = f"{folder_name}.zip"
    
    print(f"Downloading {zip_name} from Google Drive...")
    url = f"https://drive.google.com/uc?export=download&id={file_id}"
    gdown.download(url, zip_name, quiet=False)

    print(f"Extracting {zip_name} to {folder_path}...")
    os.makedirs(folder_path, exist_ok=True)
    with zipfile.ZipFile(zip_name, 'r') as zip_ref:
        zip_ref.extractall(folder_path)
    
    os.remove(zip_name)
    print(f"{folder_name} assets ready.\n")

print("All assets downloaded and extracted successfully.")
