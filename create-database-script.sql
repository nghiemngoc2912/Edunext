use master;
create database edunext;
use edunext;
CREATE TABLE users (
    id INT IDENTITY(1,1) PRIMARY KEY, 
    email VARCHAR(255) NOT NULL UNIQUE,
    password VARCHAR(255), 
    first_name NVARCHAR(100) NOT NULL, 
    last_name NVARCHAR(100) NOT NULL, 
	code NVARCHAR(10),
    role int NOT NULL, 
	is_deleted BIT DEFAULT 0,
    updated_at DATETIME DEFAULT GETDATE(),
	updated_by int
);

CREATE TABLE courses (
    id INT IDENTITY(1,1) PRIMARY KEY, 
    name NVARCHAR(255) NOT NULL, 
    code VARCHAR(50) NOT NULL UNIQUE, 
    description NVARCHAR(MAX), 
	is_deleted BIT DEFAULT 0,
    updated_at DATETIME DEFAULT GETDATE(),
	updated_by int
);

CREATE TABLE semesters (
    id INT IDENTITY(1,1) PRIMARY KEY, 
    name NVARCHAR(255) NOT NULL, 
    start_date DATE NOT NULL, 
    end_date DATE NOT NULL, 
	is_deleted BIT DEFAULT 0,
    updated_at DATETIME DEFAULT GETDATE(),
	updated_by int
);

CREATE TABLE classrooms (
    id INT IDENTITY(1,1) PRIMARY KEY, 
    course_id INT NOT NULL, 
	semester_id INT NOT NULL,  -- Kỳ học mà lớp thuộc về  
    teacher_id INT NOT NULL, 
    name NVARCHAR(255) NOT NULL, 
	is_deleted BIT DEFAULT 0,
    updated_at DATETIME DEFAULT GETDATE(),
	updated_by int,
    FOREIGN KEY (course_id) REFERENCES courses(id), 
    FOREIGN KEY (semester_id) REFERENCES semesters(id),  
    FOREIGN KEY (teacher_id) REFERENCES users(id)
);

CREATE TABLE class_enrollments (
    id INT IDENTITY(1,1) PRIMARY KEY, 
    class_id INT NOT NULL, 
    user_id INT NOT NULL, 
	is_deleted BIT DEFAULT 0,
    updated_at DATETIME DEFAULT GETDATE(),
	updated_by int,
	FOREIGN KEY (class_id) REFERENCES classrooms(id), 
    FOREIGN KEY (user_id) REFERENCES users(id)
);

CREATE TABLE slots (
    id INT IDENTITY(1,1) PRIMARY KEY, 
    course_id INT NOT NULL, 
    name NVARCHAR(255) NOT NULL,
    [order] INT NOT NULL,
	is_deleted BIT DEFAULT 0,
    updated_at DATETIME DEFAULT GETDATE(),
	updated_by int,
	FOREIGN KEY (course_id) REFERENCES courses(id)
);

CREATE TABLE class_slot_contents (
    id INT IDENTITY(1,1) PRIMARY KEY, 
    class_id INT NOT NULL, 
    slot_id INT NOT NULL,  
    FOREIGN KEY (class_id) REFERENCES classrooms(id),  
    FOREIGN KEY (slot_id) REFERENCES slots(id)
);


-- Bảng questions: Lưu câu hỏi của từng slot
CREATE TABLE questions (
    id INT IDENTITY(1,1) PRIMARY KEY, 
    class_slot_id INT NOT NULL, 
    content NVARCHAR(MAX) NOT NULL, 
    from_time DATETIME NOT NULL, 
    to_time DATETIME NOT NULL, 
    status INT DEFAULT 0, -- not start 0 start 1 end 2 delete -1
	updated_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (class_slot_id) REFERENCES class_slot_contents(id),  
);

CREATE TABLE comments (
    comment_id INT IDENTITY(1,1) PRIMARY KEY, 
    question_id INT NOT NULL, 
    user_id INT NOT NULL, 
    content NVARCHAR(MAX) NOT NULL, 
	updated_at DATETIME DEFAULT GETDATE(),
	FOREIGN KEY (question_id) REFERENCES questions(id), 
    FOREIGN KEY (user_id) REFERENCES users(id)
);

CREATE TABLE assignments (
    id INT IDENTITY(1,1) PRIMARY KEY, 
    class_slot_id INT NOT NULL, 
    title NVARCHAR(255) NOT NULL, 
    description NVARCHAR(MAX), 
    due_date DATE NOT NULL, 
	is_deleted BIT DEFAULT 0,
	updated_at DATETIME DEFAULT GETDATE(),
	FOREIGN KEY (class_slot_id) REFERENCES class_slot_contents(id)
);

CREATE TABLE assignment_submissions (
    id INT IDENTITY(1,1) PRIMARY KEY, 
    assignment_id INT NOT NULL, 
    user_id INT NOT NULL, 
    submission_date DATETIME DEFAULT GETDATE(), 
    file_link NVARCHAR(255), 
    grade DECIMAL(3,1), 
    feedback NVARCHAR(MAX), 
	updated_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (assignment_id) REFERENCES assignments(id), 
    FOREIGN KEY (user_id) REFERENCES users(id)
);

--CREATE TABLE materials (
--    material_id INT IDENTITY(1,1) PRIMARY KEY, 
--    class_id INT NOT NULL, 
--    title NVARCHAR(255) NOT NULL, 
--    description NVARCHAR(MAX), 
--    file_link NVARCHAR(255) NOT NULL, 
--	is_deleted BIT DEFAULT 0,
--	updated_at DATETIME DEFAULT GETDATE(),
--    FOREIGN KEY (class_id) REFERENCES classrooms(id), 
--);
-- Thêm người dùng (giảng viên, sinh viên, nhân viên)
INSERT INTO users (email, password, first_name, last_name,code, role) VALUES
('teacher1@fpt.edu.vn', 'pass1234', 'Nguyen', 'An','TC182981', 2),
('teacher2@fpt.edu.vn', 'pass1234', 'Tran', 'Binh','TC182982', 2),
('student1@fpt.edu.vn', 'pass1234', 'Le', 'Cuong','SV182828', 1),
('student2@fpt.edu.vn', 'pass1234', 'Pham', 'Duc','SV182829', 1),
('student3@fpt.edu.vn', 'pass1234', 'Hoang', 'Em','SV182823', 1),
('staff1@fpt.edu.vn', 'pass1234', 'Nghiem', 'Ngoc','ST186953', 3),
('staff2@fpt.edu.vn', 'pass1234', 'Nghiem', 'Bac','ST186954', 3),
('admin@fpt.edu.vn', 'pass1234', 'Admin', 'Ngoc','AD123456', 4);



-- Thêm khóa học
INSERT INTO courses (name, code, description) VALUES
(N'Lập trình Java', 'JAVA101', N'Khóa học lập trình Java từ cơ bản đến nâng cao'),
(N'Cơ sở dữ liệu', 'DB102', N'Khóa học về SQL Server và thiết kế database');

INSERT INTO semesters (name, start_date, end_date) VALUES
(N'FA2024', '2024-09-10', '2025-12-30'),
(N'SP2025', '2025-01-15', '2025-04-20');

-- Thêm lớp học
INSERT INTO classrooms (course_id, semester_id, teacher_id, name) VALUES
(1, 1, 1, N'SE1801'),
(1, 1, 2, N'SE1802'),
(2, 1, 2, N'SE1803');

-- Thêm sinh viên vào lớp học
INSERT INTO class_enrollments (class_id, user_id) VALUES
(1, 3), (1, 4), (1, 5),  -- Lớp Java A
(2, 3), (2, 5),          -- Lớp Java B
(3, 4), (3, 5);          -- Lớp DB C

INSERT INTO slots (course_id, name, [order]) VALUES
(1, N'Giới thiệu về Java', 1),
(1, N'Cấu trúc điều kiện', 2),
(1, N'Lập trình hướng đối tượng', 3),
(2, N'Câu lệnh SQL cơ bản', 1),
(2, N'Khóa chính và khóa ngoại', 2);

INSERT INTO class_slot_contents (class_id, slot_id) VALUES
-- Lớp Java A
(1, 1), (1, 2), (1, 3),
-- Lớp Java B
(2, 1), (2, 2), (2, 3),
-- Lớp DB C
(3, 4), (3, 5);
INSERT INTO questions (class_slot_id, content, from_time, to_time) VALUES
(1, N'Java là gì?', '2025-01-11 08:00:00', '2025-01-12 08:00:00'),
(2, N'Lệnh if-else trong Java hoạt động thế nào?', '2025-01-12 08:00:00', '2025-01-13 08:00:00'),
(4, N'Khóa chính là gì trong SQL?', '2025-08-16 10:00:00', '2025-08-17 10:00:00');

INSERT INTO comments (question_id, user_id, content) VALUES
(1, 3, N'Java là một ngôn ngữ lập trình hướng đối tượng!'),
(2, 4, N'if-else giúp kiểm tra điều kiện trong Java.'),
(3, 5, N'Khóa chính đảm bảo mỗi bản ghi trong bảng là duy nhất.');

INSERT INTO assignments (title, description, due_date,class_slot_id) VALUES
(N'Bài tập 1 - Viết chương trình Hello World', N'Tạo chương trình Java in ra dòng chữ Hello World.', '2025-01-20',1),
(N'Bài tập 2 - Lệnh if-else', N'Viết chương trình kiểm tra số chẵn lẻ trong Java.', '2025-01-22',2),
(N'Bài tập 1 - Truy vấn SQL', N'Viết câu lệnh SELECT lấy tất cả dữ liệu từ bảng slots.', '2025-08-25',3);

INSERT INTO assignment_submissions (assignment_id,user_id, submission_date, file_link, grade, feedback) VALUES
(2,4, '2025-01-19', 'link2.com', 8.0, N'Khá ổn nhưng cần cải thiện.'),
(3,5, '2025-01-21', 'link3.com', 7.5, N'Chưa tối ưu.');

--INSERT INTO materials (class_id, title, description, file_link) VALUES
--(1, N'Giáo trình Java', N'Tài liệu học Java cơ bản', 'javabook.com'),
--(2, N'Bài giảng if-else', N'Chi tiết về if-else trong Java', 'ifelse.com'),
--(3, N'Truy vấn SQL cơ bản', N'Tổng hợp các lệnh truy vấn SQL', 'sqlguide.com');
