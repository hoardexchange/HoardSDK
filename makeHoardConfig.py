import sys
import os

def showHelp():
    print("makeHoardConfig script usage:")
    print("\t makeHoardConfig [file_with_hoard_game_address] clientUrl clientPort [output_file]")
    print("example:")
    print("\t makeHoardConfig config\HoardGamesAddress.txt acedkewlxuu2nfnaexb4eraa.devel.hoard.exchange 8545 hoardConfig.cfg")

def loadAddress(fileName):
	file = open(fileName)
	return file.readline()

def makeConfig(addr, url, port, outFileName):
    outFile = open(outFileName,"w")
    outFile.write("{\n")
    outFile.write("\t\"GameID\":null,\n")
    outFile.write("\t\"GameBackendUrl\":\"\",\n")
    outFile.write("\t\"ClientUrl\":"+"\"http://"+url+":"+port+"\",\n")
    outFile.write("\t\"AccountsDir\":null,\n")
    outFile.write("\t\"GameCenterContract\":"+"\""+addr+"\"\n")
    outFile.write("}\n")
    outFile.close()

if len(sys.argv) == 5:
    makeConfig(loadAddress(sys.argv[1]), sys.argv[2], sys.argv[3], sys.argv[4])
else:
    showHelp()
