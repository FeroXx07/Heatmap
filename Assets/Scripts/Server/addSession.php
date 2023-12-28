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
        $table_name = $data["Table"];
        $providedId = $data["SessionId"]; // The ID provided to compare
        
        unset($data["Table"]);
        unset($data["SessionId"]); // Remove Id field from data

        // Initialize arrays to store column names and values
        $conditions = "SessionId = :SessionId";
        $query = "SELECT * FROM $table_name WHERE $conditions";

        try {
            $stmt = $conn->prepare($query); //PDO (PHP Data Objects)
            $stmt->bindParam(":SessionId", $providedId);

            // Execute the query to check if the entry exists
            if ($stmt->execute()) {
                $resultSet = $stmt->fetchAll();
                if ($resultSet && count($resultSet) > 0) {
                    $firstRow = $resultSet[0]; // Get the first row
                    $existingId = $firstRow['SessionId']; // Assuming the column name is 'Id'

                    // Compare provided ID with the ID in the database
                    if ($existingId == $providedId) {
                        echo "Provided ID matches the ID in the database for the entry.";

                        $columnName = "End"; // Replace with the actual column name to update
                        $newValue = $data["End"]; // The new value to update for the specific column
                        
                        // Construct the SQL query to update a specific column of the entry
                        $query = "UPDATE $table_name SET $columnName = :newValue WHERE SessionId = :idToUpdate";

                        try {
                            $stmt = $conn->prepare($query); // PDO (PHP Data Objects)
                
                            // Bind the new value and ID to the placeholders
                            $stmt->bindParam(":newValue", $newValue);
                            $stmt->bindParam(":idToUpdate", $existingId);
                
                            // Execute the update query
                            if ($stmt->execute()) {
                                echo "Column '$columnName' updated successfully.";
                            } else {
                                echo "Failed to update column '$columnName'.", PHP_EOL;
                            }
                        } catch (PDOException $e) {
                            echo "Error: " . $e->getMessage() . " " . $table_name, PHP_EOL;
                        }

                    } else {
                        echo "Provided ID does not match the ID in the database for the entry.";
                    }
                } else {
                    echo "No entry found for the provided data.";

                     // Entry doesn't exist, perform insertion
                     $columnNames = implode(', ', array_keys($data));
                     $columnValues = implode(', ', array_map(function($val) { return ":$val"; }, array_keys($data)));
 
                     $insertQuery = "INSERT INTO $table_name ($columnNames) VALUES ($columnValues)";
                     $stmt = $conn->prepare($insertQuery);
 
                     // Bind the values for insertion
                     foreach ($data as $key => &$value) {
                         $stmt->bindParam(":$key", $value);
                     }
 
                     // Execute the insertion query
                     if ($stmt->execute()) {
                         $lastInsertedId = $conn->lastInsertId();
                         echo "New entry added with ID: $lastInsertedId";
                     } else {
                         echo "Failed to add new entry.", PHP_EOL;
                     }
                }
            } else {
                echo "Query data execution failed.", PHP_EOL;
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
