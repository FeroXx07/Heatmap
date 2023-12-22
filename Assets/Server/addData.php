<?php
require_once 'database.php';

// Create a new Database object
$database = new Database();
$conn = $database->connect();

// Check if the connection was successful
if ($conn) {
    // Assuming you receive JSON data as a POST request
    $json_data = file_get_contents('php://input');
    $data = json_decode($json_data, true);

    if ($data) {
        $table_name = $data["table"];

        // Convert dateCreatedStamp to MySQL DATETIME format
        $dateCreatedStamp = date("Y-m-d H:i:s", strtotime($data["dateCreatedStamp"]));

        // Remove dateCreatedStamp from data array
        // Ali: Unecessary line, why would you remove and add, why not replace directly?
        //unset($data["dateCreatedStamp"]);
        unset($data["id"]);
        
        // Add the converted dateCreatedStamp back to the data array
        $data["dateCreatedStamp"] = $dateCreatedStamp;

        // Initialize arrays to store column names and values
        $columns = array();
        $column_values = array();
        
        // Iterate through the JSON data and build the column names and values dynamically
        foreach ($data as $key => $value) {         //
            $columns[] = $key;                      // $columns: Contains the column names.
            $column_values[] = ":$key";             // $column_values: Contains placeholders for the values.
        } // This array will store placeholders for the values that will be bound later. Using placeholders like :key is a good practice for preventing SQL injection.

        // Construct the SQL query to insert data
        $columns_string = implode(', ', $columns); 
        $values_string = implode(', ', $column_values); 
        $query = "INSERT INTO $table_name ($columns_string) VALUES ($values_string)";
        
        try {
            $stmt = $conn->prepare($query); //PDO (PHP Data Objects)

            // Bind the actual values to the placeholders
            foreach ($data as $key => &$value) {
                $stmt->bindParam(":$key", $value);
            }

            // Execute the query
            if ($stmt->execute()) {
                $newID = $conn->lastInsertId();
                echo "User data added successfully!. New ID is : " . $newID , PHP_EOL; 
                
            } else {
                echo "User data insertion failed.", PHP_EOL;
            }
        } catch (PDOException $e) {
            echo "Error: " . $e->getMessage() . " " . $table_name, PHP_EOL;
        }
    } else {
        echo "Invalid JSON data.", PHP_EOL;
    }
} else {
    echo "Database connection failed.", PHP_EOL;
}
?>