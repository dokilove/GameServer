START ../../PacketGanerator/bin/Debug/PacketGenerator.exe ../../PacketGanerator/PDL.xml
XCOPY /Y GenPacket.cs "../../DummyClient/Packet"
XCOPY /Y GenPacket.cs "../../Server/Packet"