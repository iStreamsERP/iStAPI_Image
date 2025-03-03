import sys
import os
import numpy as np
import face_recognition
import cv2

def process_image(image_path, output_filename):
    """Extract face encoding(s) from an image and save them as NumPy arrays."""
    
    # Validate the image path
    if not os.path.isfile(image_path):
        print(f"ERROR: The file '{image_path}' does not exist.", file=sys.stderr)
        sys.exit(1)

    # Load the image using OpenCV (BGR format)
    image = cv2.imread(image_path)
    if image is None:
        print(f"ERROR: Unable to read the image '{image_path}'.", file=sys.stderr)
        sys.exit(1)

    # Convert BGR to RGB (required by face_recognition)
    image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)

    # Extract face encodings
    encodings = face_recognition.face_encodings(image_rgb)

    # Ensure at least one face is detected
    if not encodings:
        print(f"WARNING: No face detected in '{image_path}'. Encoding not saved.", file=sys.stderr)
        sys.exit(1)

    # Get the directory of the input image
    image_dir = os.path.dirname(image_path)  

    # Save the encoding as a NumPy array in the same directory as the input image
    encoded_image_path = os.path.join(image_dir, f"{output_filename}.npy")
    np.save(encoded_image_path, encodings[0])  # Save only the first detected face encoding

    print(f"SUCCESS: Face encoding saved at: {encoded_image_path}")

if __name__ == "__main__":
    # Ensure correct arguments are provided
    if len(sys.argv) < 3:
        print("Usage: python pythonEncodeImageToNpyScript.py <image_path> <output_filename>", file=sys.stderr)
        sys.exit(1)

    image_path = sys.argv[1]
    output_filename = sys.argv[2]

    process_image(image_path, output_filename)
