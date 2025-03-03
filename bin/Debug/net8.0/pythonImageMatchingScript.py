import os
import numpy as np
from PIL import Image
from mtcnn import MTCNN
import cv2
import face_recognition
import base64
from io import BytesIO

# Function to encode a face using face_recognition
def encode_face(image):
    encodings = face_recognition.face_encodings(image)
    if encodings:
        return np.array(encodings[0])  # Ensure it's a NumPy array
    return None  # Return None if no face is detected

# Function to convert an image to Base64
def image_to_base64(image):
    buffered = BytesIO()
    image.save(buffered, format="JPEG")
    return base64.b64encode(buffered.getvalue()).decode("utf-8")

# Function to save an image as JPG and Base64
def save_image_and_base64(image, folder, filename):
    image_path = os.path.join(folder, filename + ".jpg")
    base64_path = os.path.join(folder, filename + ".txt")

    # Save as JPG
    image.save(image_path)
    
    # Convert to Base64 and save as .txt
    with open(base64_path, "w") as f:
        f.write(image_to_base64(image))

    return image_path, base64_path

# Function to find matching and non-matching faces
def find_matching_faces(image_library, capture_image, RefNo, root_folder, threshold=0.6):
    """
    Find matching faces by comparing a captured image with stored encodings.

    Parameters:
    - image_library: Path to stored face encodings
    - capture_image: Path to uploaded image
    - RefNo: Reference number from C#
    - root_folder: Base directory path from C# controller
    - threshold: Matching threshold (default = 0.6)
    """

    try:
        # Define folder paths dynamically from root_folder
        uploaded_folder = os.path.join(root_folder, '1_uploaded_images')
        detected_folder = os.path.join(root_folder, '2_detected_faces')
        matching_folder = os.path.join(root_folder, '3_matching_faces')
        non_matching_folder = os.path.join(root_folder, '4_non_matching_faces')

        # Create directories if they don’t exist
        for folder in [uploaded_folder, detected_folder, matching_folder, non_matching_folder]:
            os.makedirs(folder, exist_ok=True)

        # Load and save the uploaded image
        image = Image.open(capture_image)
        save_image_and_base64(image, uploaded_folder, os.path.splitext(os.path.basename(capture_image))[0])

        # Convert image to NumPy array for processing
        image_np = np.array(image)

        # Initialize MTCNN detector
        detector = MTCNN()

        # Load encodings from the image library
        library_encodings = {}
        for filename in os.listdir(image_library):
            if filename.endswith(".npy"):
                encoding_path = os.path.join(image_library, filename)
                loaded_encoding = np.load(encoding_path)

                if loaded_encoding.shape == (128,):  # Ensure it has the correct shape
                    library_encodings[filename] = loaded_encoding
                else:
                    print(f"Skipping {filename}: Invalid encoding shape {loaded_encoding.shape}")

        # Detect faces in the input image
        faces = detector.detect_faces(image_np)

        if faces:
            for i, face in enumerate(faces):
                x, y, width, height = face['box']
                box = (max(x, 0), max(y, 0), min(x + width, image_np.shape[1]), min(y + height, image_np.shape[0]))
                cropped_face = image.crop(box)

                # Save detected face as JPG & Base64
                detected_path, detected_base64 = save_image_and_base64(cropped_face, detected_folder, f"detected_face_{i}")

                # Convert to OpenCV format and encode detected face
                face_np = cv2.cvtColor(np.array(cropped_face), cv2.COLOR_RGB2BGR)
                img2_encoded = encode_face(face_np)

                # Skip if encoding failed
                if img2_encoded is None or img2_encoded.shape != (128,):
                    print(f"Skipping face {i}: No valid encoding found.")
                    continue

                match_found = False
                for filename, img1_encoded in library_encodings.items():
                    # Compute Euclidean distance
                    distance = np.linalg.norm(img1_encoded - img2_encoded)
                    print(f"Distance between {filename} and detected face {i}: {distance}")

                    if distance < threshold:
                        save_image_and_base64(cropped_face, matching_folder, f"{filename}")
                        match_found = True
                        break

                if not match_found:
                    save_image_and_base64(cropped_face, non_matching_folder, f"non_matching_face_{i}")

        print("Face matching process completed.")

    except Exception as e:
        print(f"Error: {e}")
