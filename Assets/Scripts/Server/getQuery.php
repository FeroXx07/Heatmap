<?php
require_once 'database.php';

// Create a new Database object
$database = new Database();
$conn = $database->connect();

// Check if the connection was successful
if ($conn) {
    // Assuming you receive JSON data as a POST request
    $query_data = file_get_contents('php://input');
    
    if ($query_data) {
        
        try {
            $stmt = $conn->prepare($query_data); //PDO (PHP Data Objects)

            // Execute the query
            if ($stmt->execute()) {
                // Initialize an empty string to store the result
                $resultString = '';

                // Fetch all rows into an associative array
                $rows = $stmt->fetchAll(PDO::FETCH_ASSOC);
                // Loop through the array of rows and concatenate data into a string
                foreach ($rows as $row) {
                    foreach ($row as $columnName => $columnValue) {
                        // Concatenate column name and value into the result string
                        $resultString .= "$columnName: $columnValue\n"; // Change "\n" to your desired line break
                    }
                    $resultString .= "\n"; // Add line break after each row for better readability
                }

                // Output the final concatenated string
                echo $resultString;
            } else {
                echo "Query execution failed", PHP_EOL;
            }
        } catch (PDOException $e) {
            echo "Error: " . $e->getMessage() . " " . $table_name, PHP_EOL;
        }
    } else {
        echo "Invalid query data", PHP_EOL;
    }
} else {
    echo "Database connection failed.", PHP_EOL;
}
?>