-- Crea las tres bases de datos
CREATE DATABASE IF NOT EXISTS `chatapp_usuarios`;
CREATE DATABASE IF NOT EXISTS `chatapp_conversaciones`;
CREATE DATABASE IF NOT EXISTS `chatapp_mensajes`;

-- Otorga todos los privilegios al usuario 'chatapp_user' sobre esas bases de datos
-- (El usuario 'chatapp_user' es creado por las variables de entorno)
GRANT ALL ON `chatapp_usuarios`.* TO 'chatapp_user'@'%';
GRANT ALL ON `chatapp_conversaciones`.* TO 'chatapp_user'@'%';
GRANT ALL ON `chatapp_mensajes`.* TO 'chatapp_user'@'%';

-- Opcional: Actualiza los privilegios
FLUSH PRIVILEGES;