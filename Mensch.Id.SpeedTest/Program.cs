using Mensch.Id.API.Tools;

var program = new MenschIdGeneratorTest();
program.ClearTestIdsFromDatabase();
await program.SpeedTest();