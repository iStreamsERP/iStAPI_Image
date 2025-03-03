
import gdown

# Google Drive file ID for facenet_keras.h5
file_id = "1XbTCC2V_L8tHqZZgLZowANoLkDB9EdOF"
url = f"https://drive.google.com/uc?id={file_id}"

# Output file name
output = "facenet_keras.h5"

# Download the file
gdown.download(url, output, quiet=False)

print("Download complete! The file is saved as", output)
