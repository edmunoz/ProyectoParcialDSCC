<?php
$name = "prueba";
$target_dir = "uploads/";
$target_file = $target_dir . basename($_FILES["file"]["image/bmp"]);
$uploadOk = 1;

$imageFileType = pathinfo($target_file,PATHINFO_EXTENSION);
// Check if image file is a actual image or fake image

    if (move_uploaded_file($_FILES["file"]["tmp_name"], $target_file)) {
        echo "The file ". basename( $_FILES["file"]["image/bmp"]). " has been uploaded.";
    } else {
        echo "Sorry, there was an error uploading your file.";
    }

?>