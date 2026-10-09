// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math;

using math = math_package;
using testing = testing_package;
using static go.math.cmplx_package;

partial class cmplx_internal_test_package {

// The higher-precision values in vc26 were used to derive the
// input arguments vc (see also comment below). For reference
// only (do not delete).
internal static slice<complex128> vc26 = new complex128[]{
    (4.9790119248836735D + 7.738872474578105D.i()),
    (7.738872474578105D + -0.2768800571920016D.i()),
    (-0.2768800571920016D + -5.010603618271075D.i()),
    (-5.010603618271075D + 9.636293707198417D.i()),
    (9.636293707198417D + 2.9263772392439646D.i()),
    (2.9263772392439646D + 5.229083431459307D.i()),
    (5.229083431459307D + 2.7279399104360103D.i()),
    (2.7279399104360103D + 1.825308091680855D.i()),
    (1.825308091680855D + -8.685924768575601D.i()),
    (-8.685924768575601D + 4.9790119248836735D.i())
}.slice();

internal static slice<complex128> vc = new complex128[]{
    (4.9790119248836735D + 7.738872474578105D.i()),
    (7.738872474578105D + -0.2768800571920016D.i()),
    (-0.2768800571920016D + -5.010603618271075D.i()),
    (-5.010603618271075D + 9.636293707198417D.i()),
    (9.636293707198417D + 2.9263772392439646D.i()),
    (2.9263772392439646D + 5.229083431459307D.i()),
    (5.229083431459307D + 2.7279399104360103D.i()),
    (2.7279399104360103D + 1.825308091680855D.i()),
    (1.825308091680855D + -8.685924768575601D.i()),
    (-8.685924768575601D + 4.9790119248836735D.i())
}.slice();

// The expected results below were computed by the high precision calculators
// at https://keisan.casio.com/.  More exact input values (array vc[], above)
// were obtained by printing them with "%.26f".  The answers were calculated
// to 26 digits (by using the "Digit number" drop-down control of each
// calculator).
internal static slice<float64> abs = new float64[]{
    9.2022120669932650313380972e+00D,
    7.7438239742296106616261394e+00D,
    5.0182478202557746902556648e+00D,
    1.0861137372799545160704002e+01D,
    1.0070841084922199607011905e+01D,
    5.9922447613166942183705192e+00D,
    5.8978784056736762299945176e+00D,
    3.2822866700678709020367184e+00D,
    8.8756430028990417290744307e+00D,
    1.0011785496777731986390856e+01D
}.slice();

internal static slice<complex128> acos = new complex128[]{
    (1.0017679804707456D + -2.9138232718554953D.i()),
    (0.036064276120414074D + 2.735858443457626D.i()),
    (1.6249365462333796D + 2.3159537454335903D.i()),
    (2.048565084965074D + -3.079557679120412D.i()),
    (0.29621132089073066D + -3.0007392508200623D.i()),
    (1.0664555914934157D + -2.487286502479601D.i()),
    (0.48681307452231387D + -2.4636559122830546D.i()),
    (0.6116977071277574D + -1.8734458851737055D.i()),
    (1.3649311280370182D + 2.8793528632328798D.i()),
    (2.6189310485682986D + -2.995654330289877D.i())
}.slice();

internal static slice<complex128> acosh = new complex128[]{
    (2.9138232718554953D + 1.0017679804707456D.i()),
    (2.735858443457626D + -0.036064276120414074D.i()),
    (2.3159537454335903D + -1.6249365462333796D.i()),
    (3.079557679120412D + 2.048565084965074D.i()),
    (3.0007392508200623D + 0.29621132089073066D.i()),
    (2.487286502479601D + 1.0664555914934157D.i()),
    (2.4636559122830546D + 0.48681307452231387D.i()),
    (1.8734458851737055D + 0.6116977071277574D.i()),
    (2.8793528632328798D + -1.3649311280370182D.i()),
    (2.995654330289877D + 2.6189310485682986D.i())
}.slice();

internal static slice<complex128> asin = new complex128[]{
    (0.569028346324151D + 2.9138232718554953D.i()),
    (1.5347320506744826D + -2.735858443457626D.i()),
    (-0.05414021943848305D + -2.3159537454335903D.i()),
    (-0.4777687581701774D + 3.079557679120412D.i()),
    (1.274585005904166D + 3.0007392508200623D.i()),
    (0.5043407353014809D + 2.487286502479601D.i()),
    (1.0839832522725827D + 2.4636559122830546D.i()),
    (0.9590986196671392D + 1.8734458851737055D.i()),
    (0.20586519875787848D + -2.8793528632328798D.i()),
    (-1.0481347217734023D + 2.995654330289877D.i())
}.slice();

internal static slice<complex128> asinh = new complex128[]{
    (2.9113760469415295D + 0.9963945954570432D.i()),
    (2.744175542399426D + -0.0354683087890005D.i()),
    (-2.296213646252069D + -1.5144663565690153D.i()),
    (-3.077123345929573D + 1.0895577967194015D.i()),
    (3.0048366100923647D + 0.2934697916981922D.i()),
    (2.4800059370795364D + 1.0545868606049165D.i()),
    (2.4718773838309587D + 0.47502344364250804D.i()),
    (1.8910743588080159D + 0.568829255725636D.i()),
    (2.873542642336734D + -1.3623761496488913D.i()),
    (-2.9981750586172478D + 0.5183571985225367D.i())
}.slice();

internal static slice<complex128> atan = new complex128[]{
    (1.5115747079332742D + 0.0913244036039545D.i()),
    (1.4424504323482603D + -0.004541613264280391D.i()),
    (-1.5593488703630534D + -0.20163295409248363D.i()),
    (-1.5280619472445889D + 0.08172155623067201D.i()),
    (1.4759909163240799D + 0.028602969320691646D.i()),
    (1.4877353772046549D + 0.1456687715320728D.i()),
    (1.420698392777919D + 0.0768304861278807D.i()),
    (1.3162236060498933D + 0.1603131300046753D.i()),
    (1.5473450684303705D + -0.11064907507939083D.i()),
    (-1.4841462340185254D + 0.0493418503050244D.i())
}.slice();

internal static slice<complex128> atanh = new complex128[]{
    (0.05837502793896851D + 1.4793488495105334D.i()),
    (0.1297734349779038D + -1.5661009410463562D.i()),
    (-0.010576456067347252D + -1.3743698658402284D.i()),
    (-0.04221859567868836D + 1.4891433968166405D.i()),
    (0.09521899799131672D + 1.541688409877711D.i()),
    (0.07996545936689033D + 1.4252510353873193D.i()),
    (0.15051245471980726D + 1.4907432533016305D.i()),
    (0.2508207293399399D + 1.3920576653921874D.i()),
    (0.022896108815797137D + -1.4609224989282865D.i()),
    (-0.08665624101841876D + 1.5207902036935093D.i())
}.slice();

internal static slice<complex128> conj = new complex128[]{
    (4.9790119248836735D + -7.738872474578105D.i()),
    (7.738872474578105D + 0.2768800571920016D.i()),
    (-0.2768800571920016D + 5.010603618271075D.i()),
    (-5.010603618271075D + -9.636293707198417D.i()),
    (9.636293707198417D + -2.9263772392439646D.i()),
    (2.9263772392439646D + -5.229083431459307D.i()),
    (5.229083431459307D + -2.7279399104360103D.i()),
    (2.7279399104360103D + -1.825308091680855D.i()),
    (1.825308091680855D + 8.685924768575601D.i()),
    (-8.685924768575601D + -4.9790119248836735D.i())
}.slice();

internal static slice<complex128> cos = new complex128[]{
    (302.4540920601484D + 1107.3797572517071D.i()),
    (0.1192858682649065D + 0.27857554122333067D.i()),
    (72.1443943045283D + -20.500129667076045D.i()),
    (2249.21952538404D + -7317.363745602774D.i()),
    (-9.148222970032421D + 1.9531246611135635D.i()),
    (-91.16081175857732D + -19.926692135699522D.i()),
    (3.7956391790427046D + 6.623513350981458D.i()),
    (-2.914484073249887D + -1.214620271628003D.i()),
    (-745.1234825012997D + 2864.169231448808D.i()),
    (-53.719779670393194D + 48.93348341339376D.i())
}.slice();

internal static slice<complex128> cosh = new complex128[]{
    (8.346383835230183D + 72.18105788642585D.i()),
    (1104.2196737991937D + -313.79638689277573D.i()),
    (0.30514852067737014D + -0.26805384730105297D.i()),
    (-73.32947286841879D + 15.744459422849182D.i()),
    (-7478.643293945957D + 1634.8382209913354D.i()),
    (4.622316522966235D + -8.088695185566376D.i()),
    (-85.44333183278877D + 37.505836120128166D.i()),
    (-1.934457815021494D + 7.372585961176723D.i()),
    (-2.352958770061749D + -2.0349820104408782D.i()),
    (779.7564575321347D + 2854.9350716819176D.i())
}.slice();

internal static slice<complex128> exp = new complex128[]{
    (16.691977368646707D + 144.36895109507662D.i()),
    (2208.4389286252585D + -627.5928928490921D.i()),
    (0.22275382731227752D + 0.724682840283342D.i()),
    (-0.006518298595815355D + -0.0013996583791519386D.i()),
    (-14957.286524084016D + 3269.6764559311355D.i()),
    (9.218158701983105D + -16.223985291084954D.i()),
    (-170.8817571685304D + 75.0138260987041D.i()),
    (-3.8524613158309595D + 14.808420423156074D.i()),
    (-4.586775503301407D + -4.178501081246873D.i()),
    (4.451337963005454e-05D + -0.00016297757420544293D.i())
}.slice();

internal static slice<complex128> log = new complex128[]{
    (2.2194438972179196D + 0.999091150469193D.i()),
    (2.046895619115417D + -0.03576257502185697D.i()),
    (1.613080832985386D + -1.625999007401906D.i()),
    (2.385191039482301D + 2.050293635965911D.i()),
    (2.3096442270679924D + 0.2948321315544676D.i()),
    (1.7904660933974657D + 1.0605860367252555D.i()),
    (1.7745926939841752D + 0.4808455608335831D.i()),
    (1.1885403350045343D + 0.5896963416477666D.i()),
    (2.1833107837679084D + -1.3636647724582456D.i()),
    (2.303762948727326D + 2.621091389538601D.i())
}.slice();

internal static slice<complex128> log10 = new complex128[]{
    (0.9638922374555904D + 0.43389977356714193D.i()),
    (0.8889554724137658D + -0.015531488990643548D.i()),
    (0.7005521046294542D + -0.7061623964948124D.i()),
    (1.0358753067322446D + 0.8904312123813498D.i()),
    (1.0030657429753302D + 0.12804396782187888D.i()),
    (0.7775895443973916D + 0.4606066633334181D.i()),
    (0.7706958146231533D + 0.20882857371769953D.i()),
    (0.5161765090119116D + 0.2561018671761598D.i()),
    (0.9481998256702664D + -0.5922320858444695D.i()),
    (1.0005115362454418D + 1.1383255270407413D.i())
}.slice();

internal partial struct ff {
    internal float64 r, theta;
}

internal static slice<ff> polar = new ff[]{
    new(9.2022120669932650313380972e+00D, 9.9909115046919291062461269e-01D),
    new(7.7438239742296106616261394e+00D, -3.5762575021856971295156489e-02D),
    new(5.0182478202557746902556648e+00D, -1.6259990074019058442232221e+00D),
    new(1.0861137372799545160704002e+01D, 2.0502936359659111755031062e+00D),
    new(1.0070841084922199607011905e+01D, 2.9483213155446756211881774e-01D),
    new(5.9922447613166942183705192e+00D, 1.0605860367252556281902109e+00D),
    new(5.8978784056736762299945176e+00D, 4.8084556083358307819310911e-01D),
    new(3.2822866700678709020367184e+00D, 5.8969634164776659423195222e-01D),
    new(8.8756430028990417290744307e+00D, -1.3636647724582455028314573e+00D),
    new(1.0011785496777731986390856e+01D, 2.6210913895386013290915234e+00D)
}.slice();

internal static slice<complex128> pow = new complex128[]{
    (-2.4999567391975295D + 1.7597517243356502D.i()),
    (73570.94338218117D + -50899.73412479152D.i()),
    (13.207772960677685D + -31.656219143339015D.i()),
    (-3.123287828297301e-07D + -1.9849567521490554e-07D.i()),
    (80622.65146847723D + -78002.8727944573D.i()),
    (-1.0268824572103166D + -0.47168447382449896D.i()),
    (-43.59538190122442D + 220.36445974645306D.i()),
    (0.8355609228325059D + -12.26157194716724D.i()),
    (1582.2929721207693D + 12735.642635242782D.i()),
    (6.592208301642123e-08D + 2.584887236651662e-08D.i())
}.slice();

internal static slice<complex128> sin = new complex128[]{
    (-1107.3801774240233D + 302.45397730025024D.i()),
    (1.031703752140076D + -0.03220897979992957D.i()),
    (-20.50195209727143D + -72.1379813482408D.i()),
    (7317.363808034634D + 2249.2195061936645D.i()),
    (-1.9643756336318081D + -9.09582647138704D.i()),
    (19.92783647158515D + -91.15557694101913D.i()),
    (-6.680335650741921D + 3.7633538331424323D.i()),
    (1.2794028166657458D + -2.7669092099795782D.i()),
    (2864.169394953526D + 745.1234399649871D.i()),
    (-48.93811726244659D + -53.71469305562194D.i())
}.slice();

internal static slice<complex128> sinh = new complex128[]{
    (8.345593533416526D + 72.18789320865079D.i()),
    (1104.2192548260646D + -313.79650595631637D.i()),
    (-0.08239469336509264D + 0.9927366875843949D.i()),
    (73.32295456982298D + -15.745859081228334D.i()),
    (-7478.643230138058D + 1634.8382349398003D.i()),
    (4.59584217901687D + -8.13529010551858D.i()),
    (-85.43842533574164D + 37.50798997857594D.i()),
    (-1.9180035008094656D + 7.435834461979351D.i()),
    (-2.233816733239658D + -2.1435190708059952D.i()),
    (-779.7564130187551D + -2854.935234659492D.i())
}.slice();

internal static slice<complex128> sqrt = new complex128[]{
    (2.662820308608613D + 1.4531345674282186D.i()),
    (2.7823278427251985D + -0.049756907317005224D.i()),
    (1.5397025302089642D + -1.6271336573016637D.i()),
    (1.7103411581506875D + 2.817067712273759D.i()),
    (3.1390392472953104D + 0.4661262584985865D.i()),
    (2.1117080764822416D + 1.2381170223514273D.i()),
    (2.3587032281672258D + 0.5782711190325734D.i()),
    (1.733526258887341D + 0.5264725822072127D.i()),
    (2.3131094974708715D + -1.8775429304303786D.i()),
    (0.8142053574504808D + 3.0575897587277248D.i())
}.slice();

internal static slice<complex128> tan = new complex128[]{
    (-1.928757919086441e-07D + 1.000000326749917D.i()),
    (1.2424126853641837D + -3.171496938831334D.i()),
    (-4.67451262515878e-05D + -0.9999243922526396D.i()),
    (4.792363401193648e-09D + 1.0000000070589334D.i()),
    (0.002345740824080089D + 0.9947733046570989D.i()),
    (-2.3960307894948155e-05D + 0.9999478134541859D.i()),
    (-0.007370204836644931D + 1.004355341341714D.i()),
    (-0.036918038479920486D + 0.9647507199346955D.i()),
    (-2.7819552567137292e-08D + -1.0000000498489106D.i()),
    (9.428159006403047e-05D + 0.9999911934086372D.i())
}.slice();

internal static slice<complex128> tanh = new complex128[]{
    (1.0000921981225144D + 2.160986245871518e-05D.i()),
    (0.9999996772753199D + -1.9953763222959658e-07D.i()),
    (-1.7654857395480372D + 1.7024216325552852D.i()),
    (-0.9999189442732737D + 3.649060704944737e-05D.i()),
    (0.9999999922462234D + -3.560088949517915e-09D.i()),
    (1.0029324933367327D + -0.004948790309797103D.i()),
    (0.9999611306478802D + -4.226995742097032e-05D.i()),
    (1.007478418931634D + -0.004194050814891698D.i()),
    (0.9938553422971833D + 0.05144217985914355D.i()),
    (-1.0000000491604983D + -2.9018731953744332e-08D.i())
}.slice();

// huge values along the real axis for testing reducePi in Tan
internal static slice<complex128> hugeIn = new complex128[]{
    2.68435456e+08D + 0D.i(),
    5.36870912e+08D + 0D.i(),
    1.073741824e+09D + 0D.i(),
    3.4359738368e+10D + 0D.i(),
    -1.329227995784916e+36D + 0D.i(),
    1.7668470647783843e+72D + 0D.i(),
    2.037035976334486e+90D + 0D.i(),
    -3.1217485503159922e+144D + 0D.i(),
    1.8919697882131776e+69D + 0D.i(),
    -2.514859209672214e+105D + 0D.i()
}.slice();

// Results for tanHuge[i] calculated with https://github.com/robpike/ivy
// using 4096 bits of working precision.
internal static slice<complex128> tanHuge = new complex128[]{
    5.95641897939639421D,
    -0.34551069233430392D,
    -0.78469661331920043D,
    0.84276385870875983D,
    0.40806638884180424D,
    -0.37603456702698076D,
    4.60901287677810962D,
    3.39135965054779932D,
    -6.76813854009065030D,
    -0.76417695016604922D
}.slice();

// special cases conform to C99 standard appendix G.6 Complex arithmetic
internal static float64 inf = math.Inf(1);
internal static float64 nan = math.NaN();

internal static slice<complex128> vcAbsSC = new complex128[]{
    NaN()
}.slice();

internal static slice<float64> absSC = new float64[]{
    math.NaN()
}.slice();

// G.6.1.1
// imaginary sign unspecified
// imaginary sign unspecified

partial struct acosSCᴛ1 /*dyn*/ {
    internal complex128 @in, want;
}
internal static slice<acosSCᴛ1> acosSC;
internal static void initᴛacosSC() { acosSC = new acosSCᴛ1[]{
    new(complex(zero, zero),
        complex(math.Pi / 2D, -zero)),
    new(complex(-zero, zero),
        complex(math.Pi / 2D, -zero)),
    new(complex(zero, nan),
        complex(math.Pi / 2D, nan)),
    new(complex(-zero, nan),
        complex(math.Pi / 2D, nan)),
    new(complex(1.0D, inf),
        complex(math.Pi / 2D, -inf)),
    new(complex(1.0D, nan),
        NaN()),
    new(complex(-inf, 1.0D),
        complex(math.Pi, -inf)),
    new(complex(inf, 1.0D),
        complex(0.0D, -inf)),
    new(complex(-inf, inf),
        complex(3D * math.Pi / 4D, -inf)),
    new(complex(inf, inf),
        complex(math.Pi / 4D, -inf)),
    new(complex(inf, nan),
        complex(nan, -inf)),
    new(complex(-inf, nan),
        complex(nan, inf)),
    new(complex(nan, 1.0D),
        NaN()),
    new(complex(nan, inf),
        complex(nan, -inf)),
    new(NaN(),
        NaN())
}.slice(); }

// G.6.2.1
internal static slice<acosSCᴛ1> acoshSC;
internal static void initᴛacoshSC() { acoshSC = new acosSCᴛ1[]{
    new(complex(zero, zero),
        complex(zero, math.Pi / 2D)),
    new(complex(-zero, zero),
        complex(zero, math.Pi / 2D)),
    new(complex(1.0D, inf),
        complex(inf, math.Pi / 2D)),
    new(complex(1.0D, nan),
        NaN()),
    new(complex(-inf, 1.0D),
        complex(inf, math.Pi)),
    new(complex(inf, 1.0D),
        complex(inf, zero)),
    new(complex(-inf, inf),
        complex(inf, 3D * math.Pi / 4D)),
    new(complex(inf, inf),
        complex(inf, math.Pi / 4D)),
    new(complex(inf, nan),
        complex(inf, nan)),
    new(complex(-inf, nan),
        complex(inf, nan)),
    new(complex(nan, 1.0D),
        NaN()),
    new(complex(nan, inf),
        complex(inf, nan)),
    new(NaN(),
        NaN())
}.slice(); }

// Derived from Asin(z) = -i * Asinh(i * z), G.6 #7
// imaginary sign unspecified
internal static slice<acosSCᴛ1> asinSC;
internal static void initᴛasinSC() { asinSC = new acosSCᴛ1[]{
    new(complex(zero, zero),
        complex(zero, zero)),
    new(complex(1.0D, inf),
        complex(0D, inf)),
    new(complex(1.0D, nan),
        NaN()),
    new(complex(inf, 1D),
        complex(math.Pi / 2D, inf)),
    new(complex(inf, inf),
        complex(math.Pi / 4D, inf)),
    new(complex(inf, nan),
        complex(nan, inf)),
    new(complex(nan, zero),
        NaN()),
    new(complex(nan, 1D),
        NaN()),
    new(complex(nan, inf),
        complex(nan, inf)),
    new(NaN(),
        NaN())
}.slice(); }

// G.6.2.2
// sign of real part unspecified
internal static slice<acosSCᴛ1> asinhSC;
internal static void initᴛasinhSC() { asinhSC = new acosSCᴛ1[]{
    new(complex(zero, zero),
        complex(zero, zero)),
    new(complex(1.0D, inf),
        complex(inf, math.Pi / 2D)),
    new(complex(1.0D, nan),
        NaN()),
    new(complex(inf, 1.0D),
        complex(inf, zero)),
    new(complex(inf, inf),
        complex(inf, math.Pi / 4D)),
    new(complex(inf, nan),
        complex(inf, nan)),
    new(complex(nan, zero),
        complex(nan, zero)),
    new(complex(nan, 1.0D),
        NaN()),
    new(complex(nan, inf),
        complex(inf, nan)),
    new(NaN(),
        NaN())
}.slice(); }

// Derived from Atan(z) = -i * Atanh(i * z), G.6 #7
internal static slice<acosSCᴛ1> atanSC;
internal static void initᴛatanSC() { atanSC = new acosSCᴛ1[]{
    new(complex(0D, zero),
        complex(0D, zero)),
    new(complex(0D, nan),
        NaN()),
    new(complex(1.0D, zero),
        complex(math.Pi / 4D, zero)),
    new(complex(1.0D, inf),
        complex(math.Pi / 2D, zero)),
    new(complex(1.0D, nan),
        NaN()),
    new(complex(inf, 1D),
        complex(math.Pi / 2D, zero)),
    new(complex(inf, inf),
        complex(math.Pi / 2D, zero)),
    new(complex(inf, nan),
        complex(math.Pi / 2D, zero)),
    new(complex(nan, 1D),
        NaN()),
    new(complex(nan, inf),
        complex(nan, zero)),
    new(NaN(),
        NaN())
}.slice(); }

// G.6.2.3
// sign of real part not specified.
internal static slice<acosSCᴛ1> atanhSC;
internal static void initᴛatanhSC() { atanhSC = new acosSCᴛ1[]{
    new(complex(zero, zero),
        complex(zero, zero)),
    new(complex(zero, nan),
        complex(zero, nan)),
    new(complex(1.0D, zero),
        complex(inf, zero)),
    new(complex(1.0D, inf),
        complex((float64)(0D), (float64)(math.Pi / 2D))),
    new(complex(1.0D, nan),
        NaN()),
    new(complex(inf, 1.0D),
        complex(zero, math.Pi / 2D)),
    new(complex(inf, inf),
        complex(zero, math.Pi / 2D)),
    new(complex(inf, nan),
        complex(0D, nan)),
    new(complex(nan, 1.0D),
        NaN()),
    new(complex(nan, inf),
        complex(zero, math.Pi / 2D)),
    new(NaN(),
        NaN())
}.slice(); }

internal static slice<complex128> vcConjSC = new complex128[]{
    NaN()
}.slice();

internal static slice<complex128> conjSC = new complex128[]{
    NaN()
}.slice();

// Derived from Cos(z) = Cosh(i * z), G.6 #7
// imaginary sign unspecified
// real sign unspecified
// imaginary sign unspecified
internal static slice<acosSCᴛ1> cosSC;
internal static void initᴛcosSC() { cosSC = new acosSCᴛ1[]{
    new(complex(zero, zero),
        complex(1.0D, -zero)),
    new(complex(zero, inf),
        complex(inf, -zero)),
    new(complex(zero, nan),
        complex(nan, zero)),
    new(complex(1.0D, inf),
        complex(inf, -inf)),
    new(complex(1.0D, nan),
        NaN()),
    new(complex(inf, zero),
        complex(nan, -zero)),
    new(complex(inf, 1.0D),
        NaN()),
    new(complex(inf, inf),
        complex(inf, nan)),
    new(complex(inf, nan),
        NaN()),
    new(complex(nan, zero),
        complex(nan, -zero)),
    new(complex(nan, 1.0D),
        NaN()),
    new(complex(nan, inf),
        complex(inf, nan)),
    new(NaN(),
        NaN())
}.slice(); }

// G.6.2.4
// imaginary sign unspecified
// imaginary sign unspecified
// +inf  cis(y)
// real sign unspecified
// imaginary sign unspecified
internal static slice<acosSCᴛ1> coshSC;
internal static void initᴛcoshSC() { coshSC = new acosSCᴛ1[]{
    new(complex(zero, zero),
        complex(1.0D, zero)),
    new(complex(zero, inf),
        complex(nan, zero)),
    new(complex(zero, nan),
        complex(nan, zero)),
    new(complex(1.0D, inf),
        NaN()),
    new(complex(1.0D, nan),
        NaN()),
    new(complex(inf, zero),
        complex(inf, zero)),
    new(complex(inf, 1.0D),
        complex(inf * math.Cos(1.0D), inf * math.Sin(1.0D))),
    new(complex(inf, inf),
        complex(inf, nan)),
    new(complex(inf, nan),
        complex(inf, nan)),
    new(complex(nan, zero),
        complex(nan, zero)),
    new(complex(nan, 1.0D),
        NaN()),
    new(complex(nan, inf),
        NaN()),
    new(NaN(),
        NaN())
}.slice(); }

// G.6.3.1
// +0 cis(y)
// +inf  cis(y)
// real and imaginary sign unspecified
// real sign unspecified
// real and imaginary sign unspecified
// real sign unspecified
internal static slice<acosSCᴛ1> expSC;
internal static void initᴛexpSC() { expSC = new acosSCᴛ1[]{
    new(complex(zero, zero),
        complex(1.0D, zero)),
    new(complex(-zero, zero),
        complex(1.0D, zero)),
    new(complex(1.0D, inf),
        NaN()),
    new(complex(1.0D, nan),
        NaN()),
    new(complex(inf, zero),
        complex(inf, zero)),
    new(complex(-inf, 1.0D),
        complex(math.Copysign(0.0D, math.Cos(1.0D)), math.Copysign(0.0D, math.Sin(1.0D)))),
    new(complex(inf, 1.0D),
        complex(inf * math.Cos(1.0D), inf * math.Sin(1.0D))),
    new(complex(-inf, inf),
        complex(zero, zero)),
    new(complex(inf, inf),
        complex(inf, nan)),
    new(complex(-inf, nan),
        complex(zero, zero)),
    new(complex(inf, nan),
        complex(inf, nan)),
    new(complex(nan, zero),
        complex(nan, zero)),
    new(complex(nan, 1.0D),
        NaN()),
    new(complex(nan, inf),
        NaN()),
    new(NaN(),
        NaN())
}.slice(); }

internal static slice<complex128> vcIsNaNSC = new complex128[]{
    complex(math.Inf(-1), math.Inf(-1)),
    complex(math.Inf(-1), math.NaN()),
    complex(math.NaN(), math.Inf(-1)),
    complex(0D, math.NaN()),
    complex(math.NaN(), 0D),
    complex(math.Inf(1), math.Inf(1)),
    complex(math.Inf(1), math.NaN()),
    complex(math.NaN(), math.Inf(1)),
    complex(math.NaN(), math.NaN())
}.slice();

internal static slice<bool> isNaNSC = new bool[]{
    false,
    false,
    false,
    true,
    true,
    false,
    false,
    false,
    true
}.slice();

// G.6.3.2
internal static slice<acosSCᴛ1> logSC;
internal static void initᴛlogSC() { logSC = new acosSCᴛ1[]{
    new(complex(zero, zero),
        complex(-inf, zero)),
    new(complex(-zero, zero),
        complex(-inf, math.Pi)),
    new(complex(1.0D, inf),
        complex(inf, math.Pi / 2D)),
    new(complex(1.0D, nan),
        NaN()),
    new(complex(-inf, 1.0D),
        complex(inf, math.Pi)),
    new(complex(inf, 1.0D),
        complex(inf, 0.0D)),
    new(complex(-inf, inf),
        complex(inf, 3D * math.Pi / 4D)),
    new(complex(inf, inf),
        complex(inf, math.Pi / 4D)),
    new(complex(-inf, nan),
        complex(inf, nan)),
    new(complex(inf, nan),
        complex(inf, nan)),
    new(complex(nan, 1.0D),
        NaN()),
    new(complex(nan, inf),
        complex(inf, nan)),
    new(NaN(),
        NaN())
}.slice(); }

// derived from Log special cases via Log10(x) = math.Log10E*Log(x)
internal static slice<acosSCᴛ1> log10SC;
internal static void initᴛlog10SC() { log10SC = new acosSCᴛ1[]{
    new(complex(zero, zero),
        complex(-inf, zero)),
    new(complex(-zero, zero),
        complex(-inf, (float64)math.Log10E * (float64)math.Pi)),
    new(complex(1.0D, inf),
        complex(inf, (float64)math.Log10E * /* math.Pi / 2 */ 1.5707963267948966D)),
    new(complex(1.0D, nan),
        NaN()),
    new(complex(-inf, 1.0D),
        complex(inf, (float64)math.Log10E * (float64)math.Pi)),
    new(complex(inf, 1.0D),
        complex(inf, 0.0D)),
    new(complex(-inf, inf),
        complex(inf, (float64)math.Log10E * /* 3 * math.Pi / 4 */ 2.356194490192345D)),
    new(complex(inf, inf),
        complex(inf, (float64)math.Log10E * /* math.Pi / 4 */ 0.7853981633974483D)),
    new(complex(-inf, nan),
        complex(inf, nan)),
    new(complex(inf, nan),
        complex(inf, nan)),
    new(complex(nan, 1.0D),
        NaN()),
    new(complex(nan, inf),
        complex(inf, nan)),
    new(NaN(),
        NaN())
}.slice(); }

internal static slice<complex128> vcPolarSC = new complex128[]{
    NaN()
}.slice();

internal static slice<ff> polarSC = new ff[]{
    new(math.NaN(), math.NaN())
}.slice();

internal static slice<array<complex128>> vcPowSC = GoReflect.WithElemDims(new array<complex128>[]{
    new complex128[]{NaN(), NaN()}.array(),
    new complex128[]{0D, NaN()}.array()
}.slice(), 2);

internal static slice<complex128> powSC = new complex128[]{
    NaN(),
    NaN()
}.slice();

// Derived from Sin(z) = -i * Sinh(i * z), G.6 #7
internal static slice<acosSCᴛ1> sinSC;
internal static void initᴛsinSC() { sinSC = new acosSCᴛ1[]{
    new(complex(zero, zero),
        complex(zero, zero)),
    new(complex(zero, inf),
        complex(zero, inf)),
    new(complex(zero, nan),
        complex(zero, nan)),
    new(complex(1.0D, inf),
        complex(inf, inf)),
    new(complex(1.0D, nan),
        NaN()),
    new(complex(inf, zero),
        complex(nan, zero)),
    new(complex(inf, 1.0D),
        NaN()),
    new(complex(inf, inf),
        complex(nan, inf)),
    new(complex(inf, nan),
        NaN()),
    new(complex(nan, zero),
        complex(nan, zero)),
    new(complex(nan, 1.0D),
        NaN()),
    new(complex(nan, inf),
        complex(nan, inf)),
    new(NaN(),
        NaN())
}.slice(); }

// G.6.2.5
// real sign unspecified
// real sign unspecified
// +inf  cis(y)
// real sign unspecified
// real sign unspecified
internal static slice<acosSCᴛ1> sinhSC;
internal static void initᴛsinhSC() { sinhSC = new acosSCᴛ1[]{
    new(complex(zero, zero),
        complex(zero, zero)),
    new(complex(zero, inf),
        complex(zero, nan)),
    new(complex(zero, nan),
        complex(zero, nan)),
    new(complex(1.0D, inf),
        NaN()),
    new(complex(1.0D, nan),
        NaN()),
    new(complex(inf, zero),
        complex(inf, zero)),
    new(complex(inf, 1.0D),
        complex(inf * math.Cos(1.0D), inf * math.Sin(1.0D))),
    new(complex(inf, inf),
        complex(inf, nan)),
    new(complex(inf, nan),
        complex(inf, nan)),
    new(complex(nan, zero),
        complex(nan, zero)),
    new(complex(nan, 1.0D),
        NaN()),
    new(complex(nan, inf),
        NaN()),
    new(NaN(),
        NaN())
}.slice(); }

// G.6.4.2
// imaginary sign unspecified
internal static slice<acosSCᴛ1> sqrtSC;
internal static void initᴛsqrtSC() { sqrtSC = new acosSCᴛ1[]{
    new(complex(zero, zero),
        complex(zero, zero)),
    new(complex(-zero, zero),
        complex(zero, zero)),
    new(complex(1.0D, inf),
        complex(inf, inf)),
    new(complex(nan, inf),
        complex(inf, inf)),
    new(complex(1.0D, nan),
        NaN()),
    new(complex(-inf, 1.0D),
        complex(zero, inf)),
    new(complex(inf, 1.0D),
        complex(inf, zero)),
    new(complex(-inf, nan),
        complex(nan, inf)),
    new(complex(inf, nan),
        complex(inf, nan)),
    new(complex(nan, 1.0D),
        NaN()),
    new(NaN(),
        NaN())
}.slice(); }

// Derived from Tan(z) = -i * Tanh(i * z), G.6 #7
internal static slice<acosSCᴛ1> tanSC;
internal static void initᴛtanSC() { tanSC = new acosSCᴛ1[]{
    new(complex(zero, zero),
        complex(zero, zero)),
    new(complex(zero, nan),
        complex(zero, nan)),
    new(complex(1.0D, inf),
        complex(zero, 1.0D)),
    new(complex(1.0D, nan),
        NaN()),
    new(complex(inf, 1.0D),
        NaN()),
    new(complex(inf, inf),
        complex(zero, 1.0D)),
    new(complex(inf, nan),
        NaN()),
    new(complex(nan, zero),
        NaN()),
    new(complex(nan, 1.0D),
        NaN()),
    new(complex(nan, inf),
        complex(zero, 1.0D)),
    new(NaN(),
        NaN())
}.slice(); }

// G.6.2.6
// 1 + i 0 sin(2y)
// imaginary sign unspecified
// imaginary sign unspecified
internal static slice<acosSCᴛ1> tanhSC;
internal static void initᴛtanhSC() { tanhSC = new acosSCᴛ1[]{
    new(complex(zero, zero),
        complex(zero, zero)),
    new(complex(1.0D, inf),
        NaN()),
    new(complex(1.0D, nan),
        NaN()),
    new(complex(inf, 1.0D),
        complex(1.0D, math.Copysign(0.0D, math.Sin(2D * 1.0D)))),
    new(complex(inf, inf),
        complex(1.0D, zero)),
    new(complex(inf, nan),
        complex(1.0D, zero)),
    new(complex(nan, zero),
        complex(nan, zero)),
    new(complex(nan, 1.0D),
        NaN()),
    new(complex(nan, inf),
        NaN()),
    new(NaN(),
        NaN())
}.slice(); }

// branch cut continuity checks
// points on each axis at |z| > 1 are checked for one-sided continuity from both the positive and negative side
// all possible branch cuts for the elementary functions are at one of these points
internal static float64 zero = 0.0D;

internal static float64 eps = 1.0D / (9007199254740992D);

internal static slice<array<complex128>> branchPoints = GoReflect.WithElemDims(new array<complex128>[]{
    new complex128[]{complex(2.0D, zero), complex(2.0D, eps)}.array(),
    new complex128[]{complex(2.0D, -zero), complex(2.0D, -eps)}.array(),
    new complex128[]{complex(-2.0D, zero), complex(-2.0D, eps)}.array(),
    new complex128[]{complex(-2.0D, -zero), complex(-2.0D, -eps)}.array(),
    new complex128[]{complex(zero, 2.0D), complex(eps, 2.0D)}.array(),
    new complex128[]{complex(-zero, 2.0D), complex(-eps, 2.0D)}.array(),
    new complex128[]{complex(zero, -2.0D), complex(eps, -2.0D)}.array(),
    new complex128[]{complex(-zero, -2.0D), complex(-eps, -2.0D)}.array()
}.slice(), 2);

// functions borrowed from pkg/math/all_test.go
internal static bool tolerance(float64 a, float64 b, float64 e) {
    var d = a - b;
    if (d < 0D) {
        d = -d;
    }
    // note: b is correct (expected) value, a is actual value.
    // make error tolerance a fraction of b, not a.
    if (b != 0D) {
        e = e * b;
        if (e < 0D) {
            e = -e;
        }
    }
    return d < e;
}

internal static bool veryclose(float64 a, float64 b) {
    return tolerance(a, b, 4e-16D);
}

internal static bool alike(float64 a, float64 b) {
    switch (ᐧ) {
    case {} when a != a && b != b: {
        return true;
    }
    case {} when a == b: {
        return math.Signbit(a) == math.Signbit(b);
    }}

    // math.IsNaN(a) && math.IsNaN(b):
    return false;
}

internal static bool cTolerance(complex128 a, complex128 b, float64 e) {
    var d = Abs(a - b);
    if (b != 0D) {
        e = e * Abs(b);
        if (e < 0D) {
            e = -e;
        }
    }
    return d < e;
}

internal static bool cSoclose(complex128 a, complex128 b, float64 e) {
    return cTolerance(a, b, e);
}

internal static bool cVeryclose(complex128 a, complex128 b) {
    return cTolerance(a, b, 4e-16D);
}

internal static bool cAlike(complex128 a, complex128 b) {
    bool realAlike = default!;
    bool imagAlike = default!;
    if (isExact(real(b))){
        realAlike = alike(real(a), real(b));
    } else {
        // Allow non-exact special cases to have errors in ULP.
        realAlike = veryclose(real(a), real(b));
    }
    if (isExact(imag(b))){
        imagAlike = alike(imag(a), imag(b));
    } else {
        // Allow non-exact special cases to have errors in ULP.
        imagAlike = veryclose(imag(a), imag(b));
    }
    return realAlike && imagAlike;
}

internal static bool isExact(float64 x) {
    // Special cases that should match exactly.  Other cases are multiples
    // of Pi that may not be last bit identical on all platforms.
    return math.IsNaN(x) || math.IsInf(x, 0) || x == 0D || x == 1D || x == -1D;
}

public static void TestAbs(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Abs(vc[i]); if (!veryclose(abs[i], f)) {
                Ꮡt.Errorf("Abs(%g) = %g, want %g"u8, vc[i], f, abs[i]);
            }
        }
    }
    for (nint i = 0; i < len(vcAbsSC); i++) {
        {
            var f = Abs(vcAbsSC[i]); if (!alike(absSC[i], f)) {
                Ꮡt.Errorf("Abs(%g) = %g, want %g"u8, vcAbsSC[i], f, absSC[i]);
            }
        }
    }
}

public static void TestAcos(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Acos(vc[i]); if (!cSoclose(acos[i], f, 1e-14D)) {
                Ꮡt.Errorf("Acos(%g) = %g, want %g"u8, vc[i], f, acos[i]);
            }
        }
    }
    foreach (var (_, v) in acosSC) {
        {
            var f = Acos(v.@in); if (!cAlike(v.want, f)) {
                Ꮡt.Errorf("Acos(%g) = %g, want %g"u8, v.@in, f, v.want);
            }
        }
        if (math.IsNaN(imag(v.@in)) || math.IsNaN(imag(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Acos(Conj(z))  == Conj(Acos(z))
        {
            var f = Acos(Conj(v.@in)); if (!cAlike(Conj(v.want), f) && !cAlike(v.@in, Conj(v.@in))) {
                Ꮡt.Errorf("Acos(%g) = %g, want %g"u8, Conj(v.@in), f, Conj(v.want));
            }
        }
    }
    foreach (var (_, vᴛ1) in branchPoints) {
        var pt = vᴛ1.Clone();

        {
            var (f0, f1) = (Acos(pt[0]), Acos(pt[1])); if (!cVeryclose(f0, f1)) {
                Ꮡt.Errorf("Acos(%g) not continuous, got %g want %g"u8, pt[0], f0, f1);
            }
        }
    }
}

public static void TestAcosh(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Acosh(vc[i]); if (!cSoclose(acosh[i], f, 1e-14D)) {
                Ꮡt.Errorf("Acosh(%g) = %g, want %g"u8, vc[i], f, acosh[i]);
            }
        }
    }
    foreach (var (_, v) in acoshSC) {
        {
            var f = Acosh(v.@in); if (!cAlike(v.want, f)) {
                Ꮡt.Errorf("Acosh(%g) = %g, want %g"u8, v.@in, f, v.want);
            }
        }
        if (math.IsNaN(imag(v.@in)) || math.IsNaN(imag(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Acosh(Conj(z))  == Conj(Acosh(z))
        {
            var f = Acosh(Conj(v.@in)); if (!cAlike(Conj(v.want), f) && !cAlike(v.@in, Conj(v.@in))) {
                Ꮡt.Errorf("Acosh(%g) = %g, want %g"u8, Conj(v.@in), f, Conj(v.want));
            }
        }
    }
    foreach (var (_, vᴛ1) in branchPoints) {
        var pt = vᴛ1.Clone();

        {
            var (f0, f1) = (Acosh(pt[0]), Acosh(pt[1])); if (!cVeryclose(f0, f1)) {
                Ꮡt.Errorf("Acosh(%g) not continuous, got %g want %g"u8, pt[0], f0, f1);
            }
        }
    }
}

public static void TestAsin(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Asin(vc[i]); if (!cSoclose(asin[i], f, 1e-14D)) {
                Ꮡt.Errorf("Asin(%g) = %g, want %g"u8, vc[i], f, asin[i]);
            }
        }
    }
    foreach (var (_, v) in asinSC) {
        {
            var f = Asin(v.@in); if (!cAlike(v.want, f)) {
                Ꮡt.Errorf("Asin(%g) = %g, want %g"u8, v.@in, f, v.want);
            }
        }
        if (math.IsNaN(imag(v.@in)) || math.IsNaN(imag(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Asin(Conj(z))  == Asin(Sinh(z))
        {
            var f = Asin(Conj(v.@in)); if (!cAlike(Conj(v.want), f) && !cAlike(v.@in, Conj(v.@in))) {
                Ꮡt.Errorf("Asin(%g) = %g, want %g"u8, Conj(v.@in), f, Conj(v.want));
            }
        }
        if (math.IsNaN(real(v.@in)) || math.IsNaN(real(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Asin(-z)  == -Asin(z)
        {
            var f = Asin(-v.@in); if (!cAlike(-v.want, f) && !cAlike(v.@in, -v.@in)) {
                Ꮡt.Errorf("Asin(%g) = %g, want %g"u8, -v.@in, f, -v.want);
            }
        }
    }
    foreach (var (_, vᴛ1) in branchPoints) {
        var pt = vᴛ1.Clone();

        {
            var (f0, f1) = (Asin(pt[0]), Asin(pt[1])); if (!cVeryclose(f0, f1)) {
                Ꮡt.Errorf("Asin(%g) not continuous, got %g want %g"u8, pt[0], f0, f1);
            }
        }
    }
}

public static void TestAsinh(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Asinh(vc[i]); if (!cSoclose(asinh[i], f, 4e-15D)) {
                Ꮡt.Errorf("Asinh(%g) = %g, want %g"u8, vc[i], f, asinh[i]);
            }
        }
    }
    foreach (var (_, v) in asinhSC) {
        {
            var f = Asinh(v.@in); if (!cAlike(v.want, f)) {
                Ꮡt.Errorf("Asinh(%g) = %g, want %g"u8, v.@in, f, v.want);
            }
        }
        if (math.IsNaN(imag(v.@in)) || math.IsNaN(imag(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Asinh(Conj(z))  == Asinh(Sinh(z))
        {
            var f = Asinh(Conj(v.@in)); if (!cAlike(Conj(v.want), f) && !cAlike(v.@in, Conj(v.@in))) {
                Ꮡt.Errorf("Asinh(%g) = %g, want %g"u8, Conj(v.@in), f, Conj(v.want));
            }
        }
        if (math.IsNaN(real(v.@in)) || math.IsNaN(real(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Asinh(-z)  == -Asinh(z)
        {
            var f = Asinh(-v.@in); if (!cAlike(-v.want, f) && !cAlike(v.@in, -v.@in)) {
                Ꮡt.Errorf("Asinh(%g) = %g, want %g"u8, -v.@in, f, -v.want);
            }
        }
    }
    foreach (var (_, vᴛ1) in branchPoints) {
        var pt = vᴛ1.Clone();

        {
            var (f0, f1) = (Asinh(pt[0]), Asinh(pt[1])); if (!cVeryclose(f0, f1)) {
                Ꮡt.Errorf("Asinh(%g) not continuous, got %g want %g"u8, pt[0], f0, f1);
            }
        }
    }
}

public static void TestAtan(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Atan(vc[i]); if (!cVeryclose(atan[i], f)) {
                Ꮡt.Errorf("Atan(%g) = %g, want %g"u8, vc[i], f, atan[i]);
            }
        }
    }
    foreach (var (_, v) in atanSC) {
        {
            var f = Atan(v.@in); if (!cAlike(v.want, f)) {
                Ꮡt.Errorf("Atan(%g) = %g, want %g"u8, v.@in, f, v.want);
            }
        }
        if (math.IsNaN(imag(v.@in)) || math.IsNaN(imag(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Atan(Conj(z))  == Conj(Atan(z))
        {
            var f = Atan(Conj(v.@in)); if (!cAlike(Conj(v.want), f) && !cAlike(v.@in, Conj(v.@in))) {
                Ꮡt.Errorf("Atan(%g) = %g, want %g"u8, Conj(v.@in), f, Conj(v.want));
            }
        }
        if (math.IsNaN(real(v.@in)) || math.IsNaN(real(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Atan(-z)  == -Atan(z)
        {
            var f = Atan(-v.@in); if (!cAlike(-v.want, f) && !cAlike(v.@in, -v.@in)) {
                Ꮡt.Errorf("Atan(%g) = %g, want %g"u8, -v.@in, f, -v.want);
            }
        }
    }
    foreach (var (_, vᴛ1) in branchPoints) {
        var pt = vᴛ1.Clone();

        {
            var (f0, f1) = (Atan(pt[0]), Atan(pt[1])); if (!cVeryclose(f0, f1)) {
                Ꮡt.Errorf("Atan(%g) not continuous, got %g want %g"u8, pt[0], f0, f1);
            }
        }
    }
}

public static void TestAtanh(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Atanh(vc[i]); if (!cVeryclose(atanh[i], f)) {
                Ꮡt.Errorf("Atanh(%g) = %g, want %g"u8, vc[i], f, atanh[i]);
            }
        }
    }
    foreach (var (_, v) in atanhSC) {
        {
            var f = Atanh(v.@in); if (!cAlike(v.want, f)) {
                Ꮡt.Errorf("Atanh(%g) = %g, want %g"u8, v.@in, f, v.want);
            }
        }
        if (math.IsNaN(imag(v.@in)) || math.IsNaN(imag(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Atanh(Conj(z))  == Conj(Atanh(z))
        {
            var f = Atanh(Conj(v.@in)); if (!cAlike(Conj(v.want), f) && !cAlike(v.@in, Conj(v.@in))) {
                Ꮡt.Errorf("Atanh(%g) = %g, want %g"u8, Conj(v.@in), f, Conj(v.want));
            }
        }
        if (math.IsNaN(real(v.@in)) || math.IsNaN(real(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Atanh(-z)  == -Atanh(z)
        {
            var f = Atanh(-v.@in); if (!cAlike(-v.want, f) && !cAlike(v.@in, -v.@in)) {
                Ꮡt.Errorf("Atanh(%g) = %g, want %g"u8, -v.@in, f, -v.want);
            }
        }
    }
    foreach (var (_, vᴛ1) in branchPoints) {
        var pt = vᴛ1.Clone();

        {
            var (f0, f1) = (Atanh(pt[0]), Atanh(pt[1])); if (!cVeryclose(f0, f1)) {
                Ꮡt.Errorf("Atanh(%g) not continuous, got %g want %g"u8, pt[0], f0, f1);
            }
        }
    }
}

public static void TestConj(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Conj(vc[i]); if (!cVeryclose(conj[i], f)) {
                Ꮡt.Errorf("Conj(%g) = %g, want %g"u8, vc[i], f, conj[i]);
            }
        }
    }
    for (nint i = 0; i < len(vcConjSC); i++) {
        {
            var f = Conj(vcConjSC[i]); if (!cAlike(conjSC[i], f)) {
                Ꮡt.Errorf("Conj(%g) = %g, want %g"u8, vcConjSC[i], f, conjSC[i]);
            }
        }
    }
}

public static void TestCos(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Cos(vc[i]); if (!cSoclose(cos[i], f, 3e-15D)) {
                Ꮡt.Errorf("Cos(%g) = %g, want %g"u8, vc[i], f, cos[i]);
            }
        }
    }
    foreach (var (_, v) in cosSC) {
        {
            var f = Cos(v.@in); if (!cAlike(v.want, f)) {
                Ꮡt.Errorf("Cos(%g) = %g, want %g"u8, v.@in, f, v.want);
            }
        }
        if (math.IsNaN(imag(v.@in)) || math.IsNaN(imag(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Cos(Conj(z))  == Cos(Cosh(z))
        {
            var f = Cos(Conj(v.@in)); if (!cAlike(Conj(v.want), f) && !cAlike(v.@in, Conj(v.@in))) {
                Ꮡt.Errorf("Cos(%g) = %g, want %g"u8, Conj(v.@in), f, Conj(v.want));
            }
        }
        if (math.IsNaN(real(v.@in)) || math.IsNaN(real(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Cos(-z)  == Cos(z)
        {
            var f = Cos(-v.@in); if (!cAlike(v.want, f) && !cAlike(v.@in, -v.@in)) {
                Ꮡt.Errorf("Cos(%g) = %g, want %g"u8, -v.@in, f, v.want);
            }
        }
    }
}

public static void TestCosh(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Cosh(vc[i]); if (!cSoclose(cosh[i], f, 2e-15D)) {
                Ꮡt.Errorf("Cosh(%g) = %g, want %g"u8, vc[i], f, cosh[i]);
            }
        }
    }
    foreach (var (_, v) in coshSC) {
        {
            var f = Cosh(v.@in); if (!cAlike(v.want, f)) {
                Ꮡt.Errorf("Cosh(%g) = %g, want %g"u8, v.@in, f, v.want);
            }
        }
        if (math.IsNaN(imag(v.@in)) || math.IsNaN(imag(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Cosh(Conj(z))  == Conj(Cosh(z))
        {
            var f = Cosh(Conj(v.@in)); if (!cAlike(Conj(v.want), f) && !cAlike(v.@in, Conj(v.@in))) {
                Ꮡt.Errorf("Cosh(%g) = %g, want %g"u8, Conj(v.@in), f, Conj(v.want));
            }
        }
        if (math.IsNaN(real(v.@in)) || math.IsNaN(real(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Cosh(-z)  == Cosh(z)
        {
            var f = Cosh(-v.@in); if (!cAlike(v.want, f) && !cAlike(v.@in, -v.@in)) {
                Ꮡt.Errorf("Cosh(%g) = %g, want %g"u8, -v.@in, f, v.want);
            }
        }
    }
}

public static void TestExp(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Exp(vc[i]); if (!cSoclose(exp[i], f, 1e-15D)) {
                Ꮡt.Errorf("Exp(%g) = %g, want %g"u8, vc[i], f, exp[i]);
            }
        }
    }
    foreach (var (_, v) in expSC) {
        {
            var f = Exp(v.@in); if (!cAlike(v.want, f)) {
                Ꮡt.Errorf("Exp(%g) = %g, want %g"u8, v.@in, f, v.want);
            }
        }
        if (math.IsNaN(imag(v.@in)) || math.IsNaN(imag(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Exp(Conj(z))  == Exp(Cosh(z))
        {
            var f = Exp(Conj(v.@in)); if (!cAlike(Conj(v.want), f) && !cAlike(v.@in, Conj(v.@in))) {
                Ꮡt.Errorf("Exp(%g) = %g, want %g"u8, Conj(v.@in), f, Conj(v.want));
            }
        }
    }
}

public static void TestIsNaN(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vcIsNaNSC); i++) {
        {
            var f = IsNaN(vcIsNaNSC[i]); if (isNaNSC[i] != f) {
                Ꮡt.Errorf("IsNaN(%v) = %v, want %v"u8, vcIsNaNSC[i], f, isNaNSC[i]);
            }
        }
    }
}

public static void TestLog(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Log(vc[i]); if (!cVeryclose(log[i], f)) {
                Ꮡt.Errorf("Log(%g) = %g, want %g"u8, vc[i], f, log[i]);
            }
        }
    }
    foreach (var (_, v) in logSC) {
        {
            var f = Log(v.@in); if (!cAlike(v.want, f)) {
                Ꮡt.Errorf("Log(%g) = %g, want %g"u8, v.@in, f, v.want);
            }
        }
        if (math.IsNaN(imag(v.@in)) || math.IsNaN(imag(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Log(Conj(z))  == Conj(Log(z))
        {
            var f = Log(Conj(v.@in)); if (!cAlike(Conj(v.want), f) && !cAlike(v.@in, Conj(v.@in))) {
                Ꮡt.Errorf("Log(%g) = %g, want %g"u8, Conj(v.@in), f, Conj(v.want));
            }
        }
    }
    foreach (var (_, vᴛ1) in branchPoints) {
        var pt = vᴛ1.Clone();

        {
            var (f0, f1) = (Log(pt[0]), Log(pt[1])); if (!cVeryclose(f0, f1)) {
                Ꮡt.Errorf("Log(%g) not continuous, got %g want %g"u8, pt[0], f0, f1);
            }
        }
    }
}

public static void TestLog10(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Log10(vc[i]); if (!cVeryclose(log10[i], f)) {
                Ꮡt.Errorf("Log10(%g) = %g, want %g"u8, vc[i], f, log10[i]);
            }
        }
    }
    foreach (var (_, v) in log10SC) {
        {
            var f = Log10(v.@in); if (!cAlike(v.want, f)) {
                Ꮡt.Errorf("Log10(%g) = %g, want %g"u8, v.@in, f, v.want);
            }
        }
        if (math.IsNaN(imag(v.@in)) || math.IsNaN(imag(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Log10(Conj(z))  == Conj(Log10(z))
        {
            var f = Log10(Conj(v.@in)); if (!cAlike(Conj(v.want), f) && !cAlike(v.@in, Conj(v.@in))) {
                Ꮡt.Errorf("Log10(%g) = %g, want %g"u8, Conj(v.@in), f, Conj(v.want));
            }
        }
    }
}

public static void TestPolar(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var (r, theta) = Polar(vc[i]); if (!veryclose(polar[i].r, r) && !veryclose(polar[i].theta, theta)) {
                Ꮡt.Errorf("Polar(%g) = %g, %g want %g, %g"u8, vc[i], r, theta, polar[i].r, polar[i].theta);
            }
        }
    }
    for (nint i = 0; i < len(vcPolarSC); i++) {
        {
            var (r, theta) = Polar(vcPolarSC[i]); if (!alike(polarSC[i].r, r) && !alike(polarSC[i].theta, theta)) {
                Ꮡt.Errorf("Polar(%g) = %g, %g, want %g, %g"u8, vcPolarSC[i], r, theta, polarSC[i].r, polarSC[i].theta);
            }
        }
    }
}

public static void TestPow(ж<testing.T> Ꮡt) {
    // Special cases for Pow(0, c).
    complex128 zero = complex(0D, 0D);
    var zeroPowers = GoReflect.WithElemDims(new array<complex128>[]{
        new complex128[]{0D, 1D + 0D.i()}.array(),
        new complex128[]{1.5D, 0D.i()}.array(),
        new complex128[]{-1.5D, complex(math.Inf(0), 0D)}.array(),
        new complex128[]{-1.5D + 1.5D.i(), Inf()}.array()
    }.slice(), 2);
    foreach (var (_, vᴛ1) in zeroPowers) {
        var zp = vᴛ1.Clone();

        {
            var f = Pow(zero, zp[0]); if (f != zp[1]) {
                Ꮡt.Errorf("Pow(%g, %g) = %g, want %g"u8, zero, zp[0], f, zp[1]);
            }
        }
    }
    complex128 a = complex(3.0D, 3.0D);
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Pow(a, vc[i]); if (!cSoclose(pow[i], f, 4e-15D)) {
                Ꮡt.Errorf("Pow(%g, %g) = %g, want %g"u8, a, vc[i], f, pow[i]);
            }
        }
    }
    for (nint i = 0; i < len(vcPowSC); i++) {
        {
            var f = Pow(vcPowSC[i][0], vcPowSC[i][1]); if (!cAlike(powSC[i], f)) {
                Ꮡt.Errorf("Pow(%g, %g) = %g, want %g"u8, vcPowSC[i][0], vcPowSC[i][1], f, powSC[i]);
            }
        }
    }
    foreach (var (_, vᴛ2) in branchPoints) {
        var pt = vᴛ2.Clone();

        {
            var (f0, f1) = (Pow(pt[0], 0.1D), Pow(pt[1], 0.1D)); if (!cVeryclose(f0, f1)) {
                Ꮡt.Errorf("Pow(%g, 0.1) not continuous, got %g want %g"u8, pt[0], f0, f1);
            }
        }
    }
}

public static void TestRect(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Rect(polar[i].r, polar[i].theta); if (!cVeryclose(vc[i], f)) {
                Ꮡt.Errorf("Rect(%g, %g) = %g want %g"u8, polar[i].r, polar[i].theta, f, vc[i]);
            }
        }
    }
    for (nint i = 0; i < len(vcPolarSC); i++) {
        {
            var f = Rect(polarSC[i].r, polarSC[i].theta); if (!cAlike(vcPolarSC[i], f)) {
                Ꮡt.Errorf("Rect(%g, %g) = %g, want %g"u8, polarSC[i].r, polarSC[i].theta, f, vcPolarSC[i]);
            }
        }
    }
}

public static void TestSin(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Sin(vc[i]); if (!cSoclose(sin[i], f, 2e-15D)) {
                Ꮡt.Errorf("Sin(%g) = %g, want %g"u8, vc[i], f, sin[i]);
            }
        }
    }
    foreach (var (_, v) in sinSC) {
        {
            var f = Sin(v.@in); if (!cAlike(v.want, f)) {
                Ꮡt.Errorf("Sin(%g) = %g, want %g"u8, v.@in, f, v.want);
            }
        }
        if (math.IsNaN(imag(v.@in)) || math.IsNaN(imag(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Sin(Conj(z))  == Conj(Sin(z))
        {
            var f = Sin(Conj(v.@in)); if (!cAlike(Conj(v.want), f) && !cAlike(v.@in, Conj(v.@in))) {
                Ꮡt.Errorf("Sinh(%g) = %g, want %g"u8, Conj(v.@in), f, Conj(v.want));
            }
        }
        if (math.IsNaN(real(v.@in)) || math.IsNaN(real(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Sin(-z)  == -Sin(z)
        {
            var f = Sin(-v.@in); if (!cAlike(-v.want, f) && !cAlike(v.@in, -v.@in)) {
                Ꮡt.Errorf("Sinh(%g) = %g, want %g"u8, -v.@in, f, -v.want);
            }
        }
    }
}

public static void TestSinh(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Sinh(vc[i]); if (!cSoclose(sinh[i], f, 2e-15D)) {
                Ꮡt.Errorf("Sinh(%g) = %g, want %g"u8, vc[i], f, sinh[i]);
            }
        }
    }
    foreach (var (_, v) in sinhSC) {
        {
            var f = Sinh(v.@in); if (!cAlike(v.want, f)) {
                Ꮡt.Errorf("Sinh(%g) = %g, want %g"u8, v.@in, f, v.want);
            }
        }
        if (math.IsNaN(imag(v.@in)) || math.IsNaN(imag(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Sinh(Conj(z))  == Conj(Sinh(z))
        {
            var f = Sinh(Conj(v.@in)); if (!cAlike(Conj(v.want), f) && !cAlike(v.@in, Conj(v.@in))) {
                Ꮡt.Errorf("Sinh(%g) = %g, want %g"u8, Conj(v.@in), f, Conj(v.want));
            }
        }
        if (math.IsNaN(real(v.@in)) || math.IsNaN(real(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Sinh(-z)  == -Sinh(z)
        {
            var f = Sinh(-v.@in); if (!cAlike(-v.want, f) && !cAlike(v.@in, -v.@in)) {
                Ꮡt.Errorf("Sinh(%g) = %g, want %g"u8, -v.@in, f, -v.want);
            }
        }
    }
}

public static void TestSqrt(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Sqrt(vc[i]); if (!cVeryclose(sqrt[i], f)) {
                Ꮡt.Errorf("Sqrt(%g) = %g, want %g"u8, vc[i], f, sqrt[i]);
            }
        }
    }
    foreach (var (_, v) in sqrtSC) {
        {
            var f = Sqrt(v.@in); if (!cAlike(v.want, f)) {
                Ꮡt.Errorf("Sqrt(%g) = %g, want %g"u8, v.@in, f, v.want);
            }
        }
        if (math.IsNaN(imag(v.@in)) || math.IsNaN(imag(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Sqrt(Conj(z)) == Conj(Sqrt(z))
        {
            var f = Sqrt(Conj(v.@in)); if (!cAlike(Conj(v.want), f) && !cAlike(v.@in, Conj(v.@in))) {
                Ꮡt.Errorf("Sqrt(%g) = %g, want %g"u8, Conj(v.@in), f, Conj(v.want));
            }
        }
    }
    foreach (var (_, vᴛ1) in branchPoints) {
        var pt = vᴛ1.Clone();

        {
            var (f0, f1) = (Sqrt(pt[0]), Sqrt(pt[1])); if (!cVeryclose(f0, f1)) {
                Ꮡt.Errorf("Sqrt(%g) not continuous, got %g want %g"u8, pt[0], f0, f1);
            }
        }
    }
}

public static void TestTan(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Tan(vc[i]); if (!cSoclose(tan[i], f, 3e-15D)) {
                Ꮡt.Errorf("Tan(%g) = %g, want %g"u8, vc[i], f, tan[i]);
            }
        }
    }
    foreach (var (_, v) in tanSC) {
        {
            var f = Tan(v.@in); if (!cAlike(v.want, f)) {
                Ꮡt.Errorf("Tan(%g) = %g, want %g"u8, v.@in, f, v.want);
            }
        }
        if (math.IsNaN(imag(v.@in)) || math.IsNaN(imag(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Tan(Conj(z))  == Conj(Tan(z))
        {
            var f = Tan(Conj(v.@in)); if (!cAlike(Conj(v.want), f) && !cAlike(v.@in, Conj(v.@in))) {
                Ꮡt.Errorf("Tan(%g) = %g, want %g"u8, Conj(v.@in), f, Conj(v.want));
            }
        }
        if (math.IsNaN(real(v.@in)) || math.IsNaN(real(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Tan(-z)  == -Tan(z)
        {
            var f = Tan(-v.@in); if (!cAlike(-v.want, f) && !cAlike(v.@in, -v.@in)) {
                Ꮡt.Errorf("Tan(%g) = %g, want %g"u8, -v.@in, f, -v.want);
            }
        }
    }
}

public static void TestTanh(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vc); i++) {
        {
            var f = Tanh(vc[i]); if (!cSoclose(tanh[i], f, 2e-15D)) {
                Ꮡt.Errorf("Tanh(%g) = %g, want %g"u8, vc[i], f, tanh[i]);
            }
        }
    }
    foreach (var (_, v) in tanhSC) {
        {
            var f = Tanh(v.@in); if (!cAlike(v.want, f)) {
                Ꮡt.Errorf("Tanh(%g) = %g, want %g"u8, v.@in, f, v.want);
            }
        }
        if (math.IsNaN(imag(v.@in)) || math.IsNaN(imag(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Tanh(Conj(z))  == Conj(Tanh(z))
        {
            var f = Tanh(Conj(v.@in)); if (!cAlike(Conj(v.want), f) && !cAlike(v.@in, Conj(v.@in))) {
                Ꮡt.Errorf("Tanh(%g) = %g, want %g"u8, Conj(v.@in), f, Conj(v.want));
            }
        }
        if (math.IsNaN(real(v.@in)) || math.IsNaN(real(v.want))) {
            // Negating NaN is undefined with regard to the sign bit produced.
            continue;
        }
        // Tanh(-z)  == -Tanh(z)
        {
            var f = Tanh(-v.@in); if (!cAlike(-v.want, f) && !cAlike(v.@in, -v.@in)) {
                Ꮡt.Errorf("Tanh(%g) = %g, want %g"u8, -v.@in, f, -v.want);
            }
        }
    }
}

// See issue 17577
public static void TestInfiniteLoopIntanSeries(ж<testing.T> Ꮡt) {
    var want = Inf();
    {
        var got = Cot(0D); if (got != want) {
            Ꮡt.Errorf("Cot(0): got %g, want %g"u8, got, want);
        }
    }
}

public static void BenchmarkAbs(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Abs(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkAcos(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Acos(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkAcosh(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Acosh(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkAsin(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Asin(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkAsinh(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Asinh(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkAtan(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Atan(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkAtanh(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Atanh(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkConj(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Conj(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkCos(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Cos(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkCosh(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Cosh(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkExp(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Exp(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkLog(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Log(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkLog10(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Log10(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkPhase(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Phase(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkPolar(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Polar(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkPow(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Pow(complex(2.5D, 3.5D), complex(2.5D, 3.5D));
    }
}

public static void BenchmarkRect(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Rect(2.5D, 1.5D);
    }
}

public static void BenchmarkSin(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Sin(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkSinh(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Sinh(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkSqrt(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Sqrt(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkTan(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Tan(complex(2.5D, 3.5D));
    }
}

public static void BenchmarkTanh(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Tanh(complex(2.5D, 3.5D));
    }
}

} // end cmplx_internal_test_package
