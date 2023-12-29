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
        $date_created = $data["DateCreated"];

        unset($data["Table"]);
        unset($data["Id"]);
        unset($data["DateCreated"]);

        // Initialize arrays to store column names and values
        $conditions = array();
        
        // Iterate through the JSON data and build the conditions dynamically
        foreach ($data as $key => $value) {
            $conditions[] = "$key = :$key"; // Conditions to check for values in respective columns
        }

        // Construct the SQL query to check if the entry exists for specific columns and values
        $conditions_string = implode(' AND ', $conditions);
        $query = "SELECT * FROM $table_name WHERE $conditions_string";

        try {
            $stmt = $conn->prepare($query); //PDO (PHP Data Objects)

            // Bind the actual values to the placeholders
            foreach ($data as $key => &$value) {
                $stmt->bindParam(":$key", $value);
            }

            // Execute the query
            if ($stmt->execute()) {
                $resultSet = $stmt->fetchAll();
                if ($resultSet && count($resultSet) > 0) {
                    // Entry already exists, retrieve the ID
                    $firstRow = $resultSet[0]; // Get the first row
                    $existingId = $firstRow['Id']; // Assuming the column name is 'Id'
                    echo "Entry already exists in the table. ID: $existingId";
                } else {
                    echo "Entry does not exist in the table for the provided data.";

                    // Entry doesn't exist, perform insertion
                    $columnNames = implode(', ', array_keys($data));
                    $columnValues = implode(', ', array_map(function($val) { return ":$val"; }, array_keys($data)));

                    // Add DateCreated to the columns and values
                    $columnNames .= ", DateCreated";
                    $columnValues .= ", :DateCreated"; // Using placeholder for DateCreated

                    $insertQuery = "INSERT INTO $table_name ($columnNames) VALUES ($columnValues)";
                    $stmt = $conn->prepare($insertQuery);

                    // Bind the values for insertion
                    foreach ($data as $key => &$value) {
                        $stmt->bindParam(":$key", $value);
                    }
                    $stmt->bindParam(":DateCreated", $date_created); // Bind DateCreated (individual bind)

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
