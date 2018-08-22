import argparse
import os

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

parser = argparse.ArgumentParser(description="Make json configuration file for HoardSDK")
parser.add_argument("--addr", dest="addr", action="store", default="0x0", help="deployed hoard game center contract address in hex format (0x1234abc)")
parser.add_argument("--addr_file", dest="addr_file", action="store", help="file with deployed hoard game center contract address")
parser.add_argument("--client-url", dest="client_url", default="localhost", action="store", help="url of a geth client (default: localhost)")
parser.add_argument("--client-port", dest="client_port", action="store", default="8545", help="port of a geth client (default: 8545)")
parser.add_argument("--out", dest="output_file", action="store", default="hoardConfig.json", help="output json configuration file (default: hoardConfig.json)")
args = parser.parse_args()

if args.addr_file:
	makeConfig(loadAddress(args.addr), args.client_url, args.client_port, args.output_file)
else:
	makeConfig(args.addr, args.client_url, args.client_port, args.output_file)
