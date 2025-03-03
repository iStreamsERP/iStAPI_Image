import json

people = [
    {"name": "Haneesh", "age": 24, "degree": "MSc IT", "roll_no": "101"},
    {"name": "Sneha", "age": 25, "degree": "MSc CT", "roll_no": "102"},
    {"name": "Rahul", "age": 23, "degree": "BSc CS", "roll_no": "103"},
    {"name": "Anjali", "age": 26, "degree": "MSc AI", "roll_no": "104"},
    {"name": "Vikram", "age": 27, "degree": "MCA", "roll_no": "105"}
]

# Print the data in JSON format
print(json.dumps(people))
