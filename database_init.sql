CREATE DATABASE IF NOT EXISTS `fpbooru`;

CREATE TABLE IF NOT EXISTS `fpbooru`.`images` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `tagids_csv` VARCHAR(255) NOT NULL,
  `imagepath_csv` VARCHAR(255) NOT NULL,
  `time_created` DATETIME(1) NOT NULL,
  `time_updated` DATETIME(1) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE INDEX `id_UNIQUE` (`id` ASC),
  INDEX `openimage` (`id` ASC, `tagids_csv` ASC, `imagepath_csv` ASC))
ENGINE = InnoDB;

CREATE TABLE IF NOT EXISTS `fpbooru`.`tags` (
  `id` INT NOT NULL AUTO_INCREMENT,
  `imageids_csv` VARCHAR(255) NOT NULL,
  `name` VARCHAR(128) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE INDEX `id_UNIQUE` (`id` ASC),
  UNIQUE INDEX `name_UNIQUE` (`name` ASC),
  INDEX `listtag` (`name` ASC, `imageids_csv` ASC))
ENGINE = InnoDB;

CREATE TABLE IF NOT EXISTS `fpbooru`.`usrs` (
  `username` varchar(255) NOT NULL,
  `password` varchar(255) NOT NULL,
  `email` varchar(255) DEFAULT NULL,
  `session` varchar(255) NOT NULL,
  PRIMARY KEY (`username`),
  UNIQUE INDEX `username_UNIQUE` (`username` ASC),
  UNIQUE INDEX `session_UNIQUE` (`session` ASC)
  INDEX `sessuser` (`username` ASC, `session` ASC))
ENGINE=InnoDB;

/*
INSERT INTO fpbooru.images (tagids_csv, imagepath_csv, time_created, time_updated) VALUES ('', '23.png,28739.png', UTC_TIMESTAMP(), UTC_TIMESTAMP());
SELECT * FROM fpbooru.images ORDER BY id DESC LIMIT 16; 
SELECT username FROM fpbooru.usrs WHERE session = "sess";
*/