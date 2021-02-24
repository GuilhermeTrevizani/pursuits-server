-- MySQL dump 10.13  Distrib 8.0.23, for Win64 (x86_64)
--
-- Host: localhost    Database: bdvidapolicial
-- ------------------------------------------------------
-- Server version	8.0.23

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `banimentos`
--

DROP TABLE IF EXISTS `banimentos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `banimentos` (
  `Codigo` int NOT NULL AUTO_INCREMENT,
  `Data` datetime NOT NULL,
  `Expiracao` datetime DEFAULT NULL,
  `Usuario` int NOT NULL,
  `SocialClub` bigint NOT NULL,
  `HardwareIdHash` bigint NOT NULL,
  `HardwareIdExHash` bigint NOT NULL,
  `Motivo` varchar(255) NOT NULL,
  `UsuarioStaff` int NOT NULL,
  PRIMARY KEY (`Codigo`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `banimentos`
--

LOCK TABLES `banimentos` WRITE;
/*!40000 ALTER TABLE `banimentos` DISABLE KEYS */;
/*!40000 ALTER TABLE `banimentos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `logs`
--

DROP TABLE IF EXISTS `logs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `logs` (
  `Codigo` int NOT NULL AUTO_INCREMENT,
  `Data` datetime NOT NULL,
  `Tipo` int NOT NULL,
  `UsuarioOrigem` int NOT NULL,
  `UsuarioDestino` int NOT NULL,
  `Descricao` text NOT NULL,
  `IPOrigem` varchar(25) NOT NULL,
  `IPDestino` varchar(25) NOT NULL,
  `SocialClubOrigem` bigint NOT NULL,
  `SocialClubDestino` bigint NOT NULL,
  `HardwareIdHashOrigem` bigint NOT NULL,
  `HardwareIdExHashOrigem` bigint NOT NULL,
  `HardwareIdHashDestino` bigint NOT NULL,
  `HardwareIdExHashDestino` bigint NOT NULL,
  PRIMARY KEY (`Codigo`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `logs`
--

LOCK TABLES `logs` WRITE;
/*!40000 ALTER TABLE `logs` DISABLE KEYS */;
/*!40000 ALTER TABLE `logs` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `parametros`
--

DROP TABLE IF EXISTS `parametros`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `parametros` (
  `Codigo` int NOT NULL AUTO_INCREMENT,
  `RecordeOnline` int NOT NULL,
  PRIMARY KEY (`Codigo`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `parametros`
--

LOCK TABLES `parametros` WRITE;
/*!40000 ALTER TABLE `parametros` DISABLE KEYS */;
INSERT INTO `parametros` VALUES (1,0);
/*!40000 ALTER TABLE `parametros` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `usuarios`
--

DROP TABLE IF EXISTS `usuarios`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `usuarios` (
  `Codigo` int NOT NULL AUTO_INCREMENT,
  `Nome` varchar(25) NOT NULL,
  `Email` varchar(100) NOT NULL,
  `Senha` varchar(128) NOT NULL,
  `DataRegistro` datetime NOT NULL,
  `SocialClubRegistro` bigint NOT NULL,
  `IPRegistro` varchar(25) NOT NULL,
  `DataUltimoAcesso` datetime NOT NULL,
  `SocialClubUltimoAcesso` bigint NOT NULL,
  `IPUltimoAcesso` varchar(25) NOT NULL,
  `Staff` int NOT NULL,
  `Level` int NOT NULL,
  `Veiculo` varchar(25) NOT NULL,
  `Skin` bigint NOT NULL,
  `HardwareIdHashRegistro` bigint NOT NULL,
  `HardwareIdExHashRegistro` bigint NOT NULL,
  `HardwareIdHashUltimoAcesso` bigint NOT NULL,
  `HardwareIdExHashUltimoAcesso` bigint NOT NULL,
  `TimeStamp` bit(1) DEFAULT b'1',
  `Helicoptero` bit(1) DEFAULT b'0',
  `DataTerminoVIP` datetime DEFAULT NULL,
  `Pintura` tinyint DEFAULT '0',
  PRIMARY KEY (`Codigo`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `usuarios`
--

LOCK TABLES `usuarios` WRITE;
/*!40000 ALTER TABLE `usuarios` DISABLE KEYS */;
/*!40000 ALTER TABLE `usuarios` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2021-02-24 17:41:58
