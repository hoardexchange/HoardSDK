namespace HoardTests.Fixtures
{
    public static class PlasmaMockupResponses
    {
        public static string GetUtxos(string address)
        {
            string data = "{{'version':'1.0','success':true,'data':[" +
                "{{'utxo_pos':1000000000,'txindex':0,'owner':'{0}','oindex':0,'currency':'0x0000000000000000000000000000000000000000','blknum':1,'amount':1000}}," +
                "{{'utxo_pos':1000000000001,'txindex':0,'owner':'{0}','oindex':1,'currency':'0x3e967151f952ec2bef08107e108747f715bb8b70','blknum':1000,'amount':49990}}," +
                "{{'utxo_pos':4000000000,'txindex':0,'owner':'{0}','oindex':0,'currency':'0x3e967151f952ec2bef08107e108747f715bb8b70','blknum':4,'amount':5000}}," +
                "{{'utxo_pos':2000000000000,'txindex':0,'owner':'{0}','oindex':0,'currency':'0x3e967151f952ec2bef08107e108747f715bb8b70','blknum':2000,'amount':1}}," +
                "{{'utxo_pos':3000000000000,'txindex':0,'owner':'{0}','oindex':0,'currency':'0x3e967151f952ec2bef08107e108747f715bb8b70','blknum':3000,'amount':1}}," +
                "{{'utxo_pos':9000000630001,'txindex':63,'owner':'{0}','oindex':1,'currency':'0x3f83c7446190ae039c54506b0f65ea8ee790ee7e','blknum':9000,'amount':49699}}," +
                "{{'utxo_pos':39000000250001,'txindex':25,'owner':'{0}','oindex':1,'currency':'0x3f83c7446190ae039c54506b0f65ea8ee790ee7e','blknum':39000,'amount':4699}}," +
                "{{'utxo_pos':61000000000001,'txindex':0,'owner':'{0}','oindex':1,'currency':'0xda636e31a9800531418213b5c799960f4585c937','blknum':61000,'amount':1740}}," +
                "{{'utxo_pos':32000000000000,'txindex':0,'owner':'{0}','oindex':0,'currency':'0xda636e31a9800531418213b5c799960f4585c937','blknum':32000,'amount':1}}," +
                "{{'utxo_pos':62000000000000,'txindex':0,'owner':'{0}','oindex':0,'currency':'0xda636e31a9800531418213b5c799960f4585c937','blknum':62000,'amount':1}}," +
                "{{'utxo_pos':63000000000000,'txindex':0,'owner':'{0}','oindex':0,'currency':'0xda636e31a9800531418213b5c799960f4585c937','blknum':63000,'amount':1}}," +
                "{{'utxo_pos':64000000000000,'txindex':0,'owner':'{0}','oindex':0,'currency':'0xda636e31a9800531418213b5c799960f4585c937','blknum':64000,'amount':1}}]}}";
            return string.Format(data, address);
        }

        public static string GetBalance(string address)
        {
            return "{'version':'1.0','success':true,'data':[" +
                "{'currency':'0x0000000000000000000000000000000000000000','amount':1000}," +
                "{'currency':'0xda636e31a9800531418213b5c799960f4585c937','amount':1744}," +
                "{'currency':'0x3f83c7446190ae039c54506b0f65ea8ee790ee7e','amount':54398}," +
                "{'currency':'0x3e967151f952ec2bef08107e108747f715bb8b70','amount':54992}]}";
        }

        public static string SubmitTransaction(string tx)
        {
            return "{'version':'1.0','success':true,'data':{'txindex':0,'txhash':'0xe79904ee24be6a4203761608b3212b1132d164fd2d148e4ad128270ba80005f5','blknum':65000}}";
        }

        public static string GetChallengeData()
        {
            return "{'data':{'exit_id':1569204449534724182880927623001191798899298823,'input_index':0,'sig':'0x1a37376e8b25ea7b882378b74fadded9819838bdb5c37ccbd7ea427d156ebbb17bb5539e45ab8b31f7b4b18a77e34a83d8c86daa6467bd017d338c6a86450e4e1b','txbytes':'0xf8f7d3c683038a418080c3808080c3808080c3808080f8c0f3946b155558903d96be696fba7c3775ef8a305f31af9400000000000000000000000000000000000000008822b1c8c1227a0000f3944da4ac003adef8f91a0c51c9d608a4af57e51f5b9400000000000000000000000000000000000000008830927f74c9de0000eb94000000000000000000000000000000000000000094000000000000000000000000000000000000000080eb94000000000000000000000000000000000000000094000000000000000000000000000000000000000080a00000000000000000000000000000000000000000000000000000000000000000'},'success':true,'version':'1.0'}";
        }
    }
}
