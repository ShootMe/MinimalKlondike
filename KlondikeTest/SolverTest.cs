using Klondike.Entities;
using Xunit;
using Xunit.Abstractions;
namespace KlondikeTest
{
    public class SolverTest
    {
        private readonly ITestOutputHelper output;

        public SolverTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        private SolveResult SolveGame(int expectedMoves, int expectedFoundation, bool includeFoundation, string deal, int drawCount = 1, int maxRounds = 10, int maxMoves = 250, int maxNodes = 50000000)
        {
            Board board = new Board(drawCount);
            if (string.IsNullOrEmpty(deal))
            {
                board.Shuffle(expectedMoves);
            }
            else
            {
                board.SetDeal(deal);
            }

            output.WriteLine(board.GetDeal());
            output.WriteLine(string.Empty);
            output.WriteLine(board.ToString());

            Assert.True(board.VerifyGameState());

            board.AllowFoundationToTableau = includeFoundation;
            SolveDetail result = board.Solve(maxMoves, maxRounds, maxNodes);

            output.WriteLine(board.MovesMadeOutput);
            output.WriteLine(result.Result.ToString());
            output.WriteLine($"({board.MovesMade},{board.CardsInFoundation},{board.TimesThroughDeck})");

            Assert.True(board.VerifyGameState());

            if (expectedMoves > 0)
            {
                Assert.Equal(expectedMoves, board.MovesMade);
            }
            if (expectedFoundation > 0)
            {
                Assert.Equal(expectedFoundation, board.CardsInFoundation);
            }
            Assert.True(maxRounds >= board.TimesThroughDeck);

            return result.Result;
        }

        [Fact]
        public void Game_Solve_Minimal_01_98_3()
        {
            SolveResult result = SolveGame(98, 52, false, "122021053133044042092074131071132062123061011022101013064091114073063082034041014024103121094113102031033134072111084032023052012081112124043104083093051054", 3, 10, 250, 300000);
            Assert.Equal(SolveResult.Minimal, result);
        }
        [Fact]
        public void Game_Solve_Minimal_02_104_1()
        {
            SolveResult result = SolveGame(104, 52, false, "042041044124023073111012123081104093031052051121094074061091083102043071132134113114131022103013062032101011014034054021112064092082133033072053084063024122", 1, 10, 250, 600000);
            Assert.Equal(SolveResult.Minimal, result);
        }
        [Fact]
        public void Game_Solve_Minimal_03_108_1()
        {
            SolveResult result = SolveGame(108, 52, false, "131101034061073072044071082043092042094132014081051124083113121052063054012103062064032111134011074093053122102033013084031114091112104021133024022041123023", 1, 10, 250, 400000);
            Assert.Equal(SolveResult.Minimal, result);
        }
        [Fact]
        public void Game_Solve_Minimal_04_129_1()
        {
            SolveResult result = SolveGame(129, 52, false, "012054062044022101102122074071041011133024043013093111082034114042131103061051063053094092072084132091112023113052032031134014123064104124121021081083033073", 1, 10, 250, 500000);
            Assert.Equal(SolveResult.Minimal, result);
        }
        [Fact]
        public void Game_Solve_Minimal_05_80_1()
        {
            SolveResult result = SolveGame(80, 52, true, "011061012013133132131021051014053042134023043033032062031034022052024041054064063044121122123124111112113114101102103104091092093094081082083084071072073074", 1, 10, 250, 10000);
            Assert.Equal(SolveResult.Minimal, result);
        }
        [Fact]
        public void Game_Solve_Minimal_06_117_1()
        {
            SolveResult result = SolveGame(117, 52, false, "093051032092074131014103064031091071104013041111053102112132063061072054052133023113082043134033044114042122124101024084021123081022083034062012121073011094", 1, 10, 250, 3500000);
            Assert.Equal(SolveResult.Minimal, result);
        }
        [Fact]
        public void Game_Solve_Minimal_07_117_1()
        {
            SolveResult result = SolveGame(117, 52, false, "044112092034103111093074114031024042091084064053052021101062073121063113011054083014122102043013132033032041051082023134012104061081124131133094123071022072", 1, 10, 250, 400000);
            Assert.Equal(SolveResult.Minimal, result);
        }
        [Fact]
        public void Game_Solve_Minimal_08_132_1()
        {
            SolveResult result = SolveGame(132, 52, false, "122102054114092084012023091124133113032022123082103101034052044011131083112051134104061064033014094021111063053062031072041024121081043073013093074071042132", 1, 10, 250, 1000000);
            Assert.Equal(SolveResult.Minimal, result);
        }
        [Fact]
        public void Game_Solve_Minimal_09_123_1()
        {
            SolveResult result = SolveGame(123, 52, false, "093011091064043131071122084051074092061031062012081103032054134013014073133072041113052053121132044082042124111023083101033021123034112024094022063102104114", 1, 10, 250, 1500000);
            Assert.Equal(SolveResult.Minimal, result);
        }
        [Fact]
        public void Game_Solve_Minimal_10_134_1()
        {
            SolveResult result = SolveGame(134, 52, false, "064083042011093052131072024091081082014041101071094073062123122114031061133034104103102043121134021054132033084013044063074032111113051124092023053022112012", 1, 10, 250, 400000);
            Assert.Equal(SolveResult.Minimal, result);
        }
        [Fact]
        public void Game_Solve_Minimal_11_108_1()
        {
            SolveResult result = SolveGame(108, 52, false, "102072022064073114071042012053044133021051093124081054013112084101123132091134113041074094131104031023014061122062011103083082063121032111024092033052043034", 1, 10, 250, 600000);
            Assert.Equal(SolveResult.Minimal, result);
        }
        [Fact]
        public void Game_Solve_Minimal_12_101_1()
        {
            SolveResult result = SolveGame(101, 52, false, "112101053072123052051081023111131032124063062041044122054021024093103033071073082121022094083074133102104084114134042113011061031013091064034014043012132092", 1, 10, 250, 1600000);
            Assert.Equal(SolveResult.Minimal, result);
        }
        [Fact]
        public void Game_Solve_Minimal_12_102_1()
        {
            SolveResult result = SolveGame(102, 52, false, "112101053072123052051081023111131032124063062041044122054021024093103033071073082121022094083074133102104084114134042113011061031013091064034014043012132092", 1, 1, 250, 1800000);
            Assert.Equal(SolveResult.Minimal, result);
        }
        [Fact]
        public void Game_Solve_Minimal_13_115_1()
        {
            SolveResult result = SolveGame(115, 52, false, "081054022072134033082024052064053012061013042093084124092122062031083121113023043074051114091014103044131063041102101133011111071073034123104112021132032094", 1, 10, 250, 8000000);
            Assert.Equal(SolveResult.Minimal, result);
        }
        [Fact]
        public void Game_Solve_Minimal_14_139_1()
        {
            SolveResult result = SolveGame(139, 52, false, "073041124122091063054034021032132093111031043064013022131103123092113074083072044114062102024134082053011033042012014104084023094112121081051071052133061101", 1, 10, 250, 200000);
            Assert.Equal(SolveResult.Minimal, result);
        }
        [Fact]
        public void Game_Solve_Minimal_15_132_1()
        {
            SolveResult result = SolveGame(132, 52, false, "031133054011034033084074013043104083113014052122022053114041081062021071012024111093064032123132121072063112134131101091044103082092094124102073042023061051", 1, 10, 250, 300000);
            Assert.Equal(SolveResult.Minimal, result);
        }
        [Fact]
        public void Game_Solve_Impossible_01_17_1()
        {
            SolveResult result = SolveGame(0, 17, false, "041023134124133071102114063072031033121092131043082044122083084064061051093094054073042013091104032101113062022103052034021132012074112111123011053024081014", 1, 10, 250, 600000);
            Assert.Equal(SolveResult.Impossible, result);
        }
    }
}