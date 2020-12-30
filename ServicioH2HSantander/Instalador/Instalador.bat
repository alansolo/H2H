@echo off


set pathServicios=%CD%\

echo -------------------------------------------------------------------------------------------------------
echo 1 de 1 (Servicio Calidad)-------------------------------------------------

set nombreServicio=ServicioH2HSantander
set nombreMostrarServicio=Servico H2H Santander
set descripcionServicio=Encripta y envia archivos al Front de Santander con ApiSantander
set ejecutableServicio=%pathServicios%ServicioH2HSantander.exe


echo Instalando el servicio (1 de 2) "%nombreMostrarServicio%"
sc create %nombreServicio% binPath= "%ejecutableServicio%" DisplayName= "%nombreMostrarServicio%" type= own start= auto

echo Cambiando descripcion del servicio "%nombreMostrarServicio%"
sc description %nombreServicio% "%descripcionServicio%"

rem echo Iniciando el servicio "%nombreMostrarServicio%"
net start %nombreServicio%

echo Finalizo la instalacion ServicioH2HSantander

pause