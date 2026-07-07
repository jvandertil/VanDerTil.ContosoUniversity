CREATE TABLE courses (
    id            INT  NOT NULL GENERATED ALWAYS AS IDENTITY,
    title         TEXT NOT NULL,
    credits       INT  NOT NULL,
    department_id INT  NOT NULL,

    CONSTRAINT pk_courses PRIMARY KEY (id)
);

CREATE TABLE course_assignments (
    course_id     INT NOT NULL,
    instructor_id INT NOT NULL,

    CONSTRAINT pk_course_assignments PRIMARY KEY (course_id, instructor_id)
);

CREATE TABLE departments (
    id            INT            NOT NULL GENERATED ALWAYS AS IDENTITY,
    name          TEXT           NOT NULL,
    budget        NUMERIC(12, 2) NOT NULL,
    start_date    DATE           NOT NULL,
    instructor_id INT            NULL,

    CONSTRAINT pk_departments PRIMARY KEY (id)
);

CREATE TABLE enrollments (
    id         INT NOT NULL GENERATED ALWAYS AS IDENTITY,
    course_id  INT NOT NULL,
    student_id INT NOT NULL,
    grade      INT NULL,

    CONSTRAINT pk_enrollments PRIMARY KEY (id)
);

CREATE TABLE instructors (
    id         INT  NOT NULL GENERATED ALWAYS AS IDENTITY,
    first_name TEXT NOT NULL,
    last_name  TEXT NOT NULL,
    hire_date  DATE NOT NULL,

    CONSTRAINT pk_instructors PRIMARY KEY (id)
);

CREATE TABLE office_assignments (
    instructor_id INT  NOT NULL,
    location      TEXT NOT NULL,

    CONSTRAINT pk_office_assignments PRIMARY KEY (instructor_id)
);

CREATE TABLE students (
    id              INT  NOT NULL GENERATED ALWAYS AS IDENTITY,
    first_name      TEXT NOT NULL,
    last_name       TEXT NOT NULL,
    enrollment_date DATE NOT NULL,

    CONSTRAINT pk_students PRIMARY KEY (id)
);

ALTER TABLE courses
    ADD CONSTRAINT fk_courses_department_id FOREIGN KEY (department_id) REFERENCES departments (id);

ALTER TABLE course_assignments
    ADD CONSTRAINT fk_course_assignments_course_id FOREIGN KEY (course_id) REFERENCES courses (id);

ALTER TABLE course_assignments
    ADD CONSTRAINT fk_course_assignments_instructor_id FOREIGN KEY (instructor_id) REFERENCES instructors (id);

ALTER TABLE departments
    ADD CONSTRAINT fk_departments_instructor_id FOREIGN KEY (instructor_id) REFERENCES instructors (id);

ALTER TABLE enrollments
    ADD CONSTRAINT fk_enrollments_course_id FOREIGN KEY (course_id) REFERENCES courses (id);

ALTER TABLE enrollments
    ADD CONSTRAINT fk_enrollments_student_id FOREIGN KEY (student_id) REFERENCES students (id);

ALTER TABLE office_assignments
    ADD CONSTRAINT fk_office_assignments_instructor_id FOREIGN KEY (instructor_id) REFERENCES instructors (id);
