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



-- Create models table with ModelId
CREATE TABLE models (
    ModelId INT PRIMARY KEY IDENTITY(1,1),
    name NVARCHAR(100) UNIQUE NOT NULL,
    bio TEXT,
    profile_pic NVARCHAR(500),
    is_active BIT DEFAULT 1,
    created_at DATETIME DEFAULT GETDATE()
);

-- Create videos table with VideoId
CREATE TABLE videos (
    VideoId INT PRIMARY KEY IDENTITY(1,1),
    title NVARCHAR(200) NOT NULL,
    description TEXT,
    file_path NVARCHAR(500) NOT NULL,
    thumbnail_path NVARCHAR(500),
    duration INT,
    file_size BIGINT,
    views INT DEFAULT 0,
    model_id INT FOREIGN KEY REFERENCES models(ModelId) ON DELETE CASCADE,
    uploaded_by INT FOREIGN KEY REFERENCES users(UserId),
    uploaded_at DATETIME DEFAULT GETDATE(),
    tags NVARCHAR(500),
    is_approved BIT DEFAULT 1,
    is_featured BIT DEFAULT 0
);

