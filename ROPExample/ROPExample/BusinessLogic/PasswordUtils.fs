module PasswordUtils

let ComparePasswords password1 password2 = 
    if (password1 = password2) then
        Ok ()
    else
        Error ("Passwords do not match")