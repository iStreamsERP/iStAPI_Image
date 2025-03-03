import sys
import os
import numpy as np
import face_recognition
import cv2

def process_image(image_path, outputfilename):
    """Extract face encoding from an image and save it as a NumPy array."""

    # Validate the image path
    if not os.path.isfile(image_path):
        print(f"Error: The file '{image_path}' does not exist.")
        return

    # Load the image using OpenCV (BGR format)
    image = cv2.imread(image_path)
    if image is None:
        print(f"Error: Unable to read the image '{image_path}'.")
        return

    # Convert BGR to RGB (required by face_recognition)
    image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)

    # Extract face encodings
    encodings = face_recognition.face_encodings(image_rgb)

    # Ensure a face was found
    if not encodings:
        print(f"No face detected in '{image_path}'. Encoding not saved.")
        return

    # Use only the first detected face encoding (128-dimensional vector)
    encoding = np.array(encodings[0])

    # Define the output directory
    image_dir = os.path.dirname(image_path)  # Extract directory of image
    encode_directory = os.path.join(image_dir, "encodeimage")

    # Ensure the output directory exists
    os.makedirs(encode_directory, exist_ok=True)

    # Create the full path for the encoded file
    encoded_image_path = os.path.join(encode_directory, f"{outputfilename}.npy")

    # Save the encoding as a NumPy array
    np.save(encoded_image_path, encoding)
    print(f"Face encoding saved at: {encoded_image_path}")

if __name__ == "__main__":
    # Ensure correct arguments are provided
    if len(sys.argv) < 3:
        print("Usage: python pythonEncodeImageToNpyScript.py <image_path> <output_filename>")
        sys.exit(1)

    image_path = sys.argv[1]
    outputfilename = sys.argv[2]

    process_image(image_path, outputfilename)
