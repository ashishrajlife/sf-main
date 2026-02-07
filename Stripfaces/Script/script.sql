CREATE TABLE users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    username NVARCHAR(50) UNIQUE NOT NULL,
    email NVARCHAR(100) UNIQUE NOT NULL,
    password NVARCHAR(255) NOT NULL,
    role NVARCHAR(10) DEFAULT 'user' CHECK (role IN ('admin', 'user')),
    created_at DATETIME DEFAULT GETDATE()
);

-- Insert default admin (password: Admin@123)
INSERT INTO users (username, email, password, role) 
VALUES ('admin', 'admin@stream.com', 'kbTRQoI/fSDF8I32kSLeQ/NfBXqYjZYZ9tMThIXJogM=', 'admin');

-- Insert sample user (password: User@123)
INSERT INTO users (username, email, password, role) 
VALUES ('usertest', 'user1@gmail.com', 'kbTRQoI/fSDF8I32kSLeQ/NfBXqYjZYZ9tMThIXJogM=', 'user');