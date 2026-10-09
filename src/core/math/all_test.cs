// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using static math_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static go.math_internal_test_package;

partial class math_test_package {

internal static slice<float64> vf = new float64[]{
    4.9790119248836735e+00D,
    7.7388724745781045e+00D,
    -2.7688005719200159e-01D,
    -5.0106036182710749e+00D,
    9.6362937071984173e+00D,
    2.9263772392439646e+00D,
    5.2290834314593066e+00D,
    2.7279399104360102e+00D,
    1.8253080916808550e+00D,
    -8.6859247685756013e+00D
}.slice();

// The expected results below were computed by the high precision calculators
// at https://keisan.casio.com/.  More exact input values (array vf[], above)
// were obtained by printing them with "%.26f".  The answers were calculated
// to 26 digits (by using the "Digit number" drop-down control of each
// calculator).
internal static slice<float64> acos = new float64[]{
    1.0496193546107222142571536e+00D,
    6.8584012813664425171660692e-01D,
    1.5984878714577160325521819e+00D,
    2.0956199361475859327461799e+00D,
    2.7053008467824138592616927e-01D,
    1.2738121680361776018155625e+00D,
    1.0205369421140629186287407e+00D,
    1.2945003481781246062157835e+00D,
    1.3872364345374451433846657e+00D,
    2.6231510803970463967294145e+00D
}.slice();

internal static slice<float64> acosh = new float64[]{
    2.4743347004159012494457618e+00D,
    2.8576385344292769649802701e+00D,
    7.2796961502981066190593175e-01D,
    2.4796794418831451156471977e+00D,
    3.0552020742306061857212962e+00D,
    2.044238592688586588942468e+00D,
    2.5158701513104513595766636e+00D,
    1.99050839282411638174299e+00D,
    1.6988625798424034227205445e+00D,
    2.9611454842470387925531875e+00D
}.slice();

internal static slice<float64> asin = new float64[]{
    5.2117697218417440497416805e-01D,
    8.8495619865825236751471477e-01D,
    -0.027691544662819412D,
    -5.2482360935268931351485822e-01D,
    1.3002662421166552333051524e+00D,
    2.9698415875871901741575922e-01D,
    5.5025938468083370060258102e-01D,
    2.7629597861677201301553823e-01D,
    1.83559892257451475846656e-01D,
    -1.0523547536021497774980928e+00D
}.slice();

internal static slice<float64> asinh = new float64[]{
    2.3083139124923523427628243e+00D,
    2.743551594301593620039021e+00D,
    -2.7345908534880091229413487e-01D,
    -2.3145157644718338650499085e+00D,
    2.9613652154015058521951083e+00D,
    1.7949041616585821933067568e+00D,
    2.3564032905983506405561554e+00D,
    1.7287118790768438878045346e+00D,
    1.3626658083714826013073193e+00D,
    -2.8581483626513914445234004e+00D
}.slice();

internal static slice<float64> atan = new float64[]{
    1.372590262129621651920085e+00D,
    1.442290609645298083020664e+00D,
    -2.7011324359471758245192595e-01D,
    -1.3738077684543379452781531e+00D,
    1.4673921193587666049154681e+00D,
    1.2415173565870168649117764e+00D,
    1.3818396865615168979966498e+00D,
    1.2194305844639670701091426e+00D,
    1.0696031952318783760193244e+00D,
    -1.4561721938838084990898679e+00D
}.slice();

internal static slice<float64> atanh = new float64[]{
    5.4651163712251938116878204e-01D,
    1.0299474112843111224914709e+00D,
    -2.7695084420740135145234906e-02D,
    -5.5072096119207195480202529e-01D,
    1.9943940993171843235906642e+00D,
    3.01448604578089708203017e-01D,
    5.8033427206942188834370595e-01D,
    2.7987997499441511013958297e-01D,
    1.8459947964298794318714228e-01D,
    -1.3273186910532645867272502e+00D
}.slice();

internal static slice<float64> atan2 = new float64[]{
    1.1088291730037004444527075e+00D,
    9.1218183188715804018797795e-01D,
    1.5984772603216203736068915e+00D,
    2.0352918654092086637227327e+00D,
    8.0391819139044720267356014e-01D,
    1.2861075249894661588866752e+00D,
    1.0889904479131695712182587e+00D,
    1.3044821793397925293797357e+00D,
    1.3902530903455392306872261e+00D,
    2.2859857424479142655411058e+00D
}.slice();

internal static slice<float64> cbrt = new float64[]{
    1.7075799841925094446722675e+00D,
    1.9779982212970353936691498e+00D,
    -6.5177429017779910853339447e-01D,
    -1.7111838886544019873338113e+00D,
    2.1279920909827937423960472e+00D,
    1.4303536770460741452312367e+00D,
    1.7357021059106154902341052e+00D,
    1.3972633462554328350552916e+00D,
    1.2221149580905388454977636e+00D,
    -2.0556003730500069110343596e+00D
}.slice();

internal static slice<float64> ceil = new float64[]{
    5.0000000000000000e+00D,
    8.0000000000000000e+00D,
    Copysign(0D, -1D),
    -5.0000000000000000e+00D,
    1.0000000000000000e+01D,
    3.0000000000000000e+00D,
    6.0000000000000000e+00D,
    3.0000000000000000e+00D,
    2.0000000000000000e+00D,
    -8.0000000000000000e+00D
}.slice();

internal static slice<float64> copysign = new float64[]{
    -4.9790119248836735e+00D,
    -7.7388724745781045e+00D,
    -2.7688005719200159e-01D,
    -5.0106036182710749e+00D,
    -9.6362937071984173e+00D,
    -2.9263772392439646e+00D,
    -5.2290834314593066e+00D,
    -2.7279399104360102e+00D,
    -1.8253080916808550e+00D,
    -8.6859247685756013e+00D
}.slice();

internal static slice<float64> cos = new float64[]{
    2.634752140995199110787593e-01D,
    1.148551260848219865642039e-01D,
    9.6191297325640768154550453e-01D,
    2.938141150061714816890637e-01D,
    -9.777138189897924126294461e-01D,
    -9.7693041344303219127199518e-01D,
    4.940088096948647263961162e-01D,
    -9.1565869021018925545016502e-01D,
    -2.517729313893103197176091e-01D,
    -7.39241351595676573201918e-01D
}.slice();

// Results for 100000 * Pi + vf[i]
internal static slice<float64> cosLarge = new float64[]{
    2.634752141185559426744e-01D,
    1.14855126055543100712e-01D,
    9.61912973266488928113e-01D,
    2.9381411499556122552e-01D,
    -9.777138189880161924641e-01D,
    -9.76930413445147608049e-01D,
    4.940088097314976789841e-01D,
    -9.15658690217517835002e-01D,
    -2.51772931436786954751e-01D,
    -7.3924135157173099849e-01D
}.slice();

internal static slice<float64> cosh = new float64[]{
    7.2668796942212842775517446e+01D,
    1.1479413465659254502011135e+03D,
    1.0385767908766418550935495e+00D,
    7.5000957789658051428857788e+01D,
    7.655246669605357888468613e+03D,
    9.3567491758321272072888257e+00D,
    9.331351599270605471131735e+01D,
    7.6833430994624643209296404e+00D,
    3.1829371625150718153881164e+00D,
    2.9595059261916188501640911e+03D
}.slice();

internal static slice<float64> erf = new float64[]{
    5.1865354817738701906913566e-01D,
    7.2623875834137295116929844e-01D,
    -3.123458688281309990629839e-02D,
    -5.2143121110253302920437013e-01D,
    8.2704742671312902508629582e-01D,
    3.2101767558376376743993945e-01D,
    5.403990312223245516066252e-01D,
    3.0034702916738588551174831e-01D,
    2.0369924417882241241559589e-01D,
    -7.8069386968009226729944677e-01D
}.slice();

internal static slice<float64> erfc = new float64[]{
    4.8134645182261298093086434e-01D,
    2.7376124165862704883070156e-01D,
    1.0312345868828130999062984e+00D,
    1.5214312111025330292043701e+00D,
    1.7295257328687097491370418e-01D,
    6.7898232441623623256006055e-01D,
    4.596009687776754483933748e-01D,
    6.9965297083261411448825169e-01D,
    7.9630075582117758758440411e-01D,
    1.7806938696800922672994468e+00D
}.slice();

internal static slice<float64> erfinv = new float64[]{
    4.746037673358033586786350696e-01D,
    8.559054432692110956388764172e-01D,
    -2.45427830571707336251331946e-02D,
    -4.78116683518973366268905506e-01D,
    1.479804430319470983648120853e+00D,
    2.654485787128896161882650211e-01D,
    5.027444534221520197823192493e-01D,
    2.466703532707627818954585670e-01D,
    1.632011465103005426240343116e-01D,
    -1.06672334642196900710000389e+00D
}.slice();

internal static slice<float64> exp = new float64[]{
    1.4533071302642137507696589e+02D,
    2.2958822575694449002537581e+03D,
    7.5814542574851666582042306e-01D,
    6.6668778421791005061482264e-03D,
    1.5310493273896033740861206e+04D,
    1.8659907517999328638667732e+01D,
    1.8662167355098714543942057e+02D,
    1.5301332413189378961665788e+01D,
    6.2047063430646876349125085e+00D,
    1.6894712385826521111610438e-04D
}.slice();

internal static slice<float64> expm1 = new float64[]{
    5.105047796122957327384770212e-02D,
    8.046199708567344080562675439e-02D,
    -2.764970978891639815187418703e-03D,
    -4.8871434888875355394330300273e-02D,
    1.0115864277221467777117227494e-01D,
    2.969616407795910726014621657e-02D,
    5.368214487944892300914037972e-02D,
    2.765488851131274068067445335e-02D,
    1.842068661871398836913874273e-02D,
    -8.3193870863553801814961137573e-02D
}.slice();

internal static slice<float64> expm1Large = new float64[]{
    4.2031418113550844e+21D,
    4.0690789717473863e+33D,
    -0.9372627915981363e+00D,
    -1.0D,
    7.077694784145933e+41D,
    5.117936223839153e+12D,
    5.124137759001189e+22D,
    7.03546003972584e+11D,
    8.456921800389698e+07D,
    -1.0D
}.slice();

internal static slice<float64> exp2 = new float64[]{
    3.1537839463286288034313104e+01D,
    2.1361549283756232296144849e+02D,
    8.2537402562185562902577219e-01D,
    3.1021158628740294833424229e-02D,
    7.9581744110252191462569661e+02D,
    7.6019905892596359262696423e+00D,
    3.7506882048388096973183084e+01D,
    6.6250893439173561733216375e+00D,
    3.5438267900243941544605339e+00D,
    2.4281533133513300984289196e-03D
}.slice();

internal static slice<float64> fabs = new float64[]{
    4.9790119248836735e+00D,
    7.7388724745781045e+00D,
    2.7688005719200159e-01D,
    5.0106036182710749e+00D,
    9.6362937071984173e+00D,
    2.9263772392439646e+00D,
    5.2290834314593066e+00D,
    2.7279399104360102e+00D,
    1.8253080916808550e+00D,
    8.6859247685756013e+00D
}.slice();

internal static slice<float64> fdim = new float64[]{
    4.9790119248836735e+00D,
    7.7388724745781045e+00D,
    0.0000000000000000e+00D,
    0.0000000000000000e+00D,
    9.6362937071984173e+00D,
    2.9263772392439646e+00D,
    5.2290834314593066e+00D,
    2.7279399104360102e+00D,
    1.8253080916808550e+00D,
    0.0000000000000000e+00D
}.slice();

internal static slice<float64> floor = new float64[]{
    4.0000000000000000e+00D,
    7.0000000000000000e+00D,
    -1.0000000000000000e+00D,
    -6.0000000000000000e+00D,
    9.0000000000000000e+00D,
    2.0000000000000000e+00D,
    5.0000000000000000e+00D,
    2.0000000000000000e+00D,
    1.0000000000000000e+00D,
    -9.0000000000000000e+00D
}.slice();

internal static slice<float64> fmod = new float64[]{
    4.197615023265299782906368e-02D,
    2.261127525421895434476482e+00D,
    3.231794108794261433104108e-02D,
    4.989396381728925078391512e+00D,
    3.637062928015826201999516e-01D,
    1.220868282268106064236690e+00D,
    4.770916568540693347699744e+00D,
    1.816180268691969246219742e+00D,
    8.734595415957246977711748e-01D,
    1.314075231424398637614104e+00D
}.slice();

partial struct fi {
    internal float64 f;
    internal nint i;
}

internal static slice<fi> frexp = new fi[]{
    new(6.2237649061045918750e-01D, 3),
    new(9.6735905932226306250e-01D, 3),
    new(-5.5376011438400318000e-01D, -1),
    new(-6.2632545228388436250e-01D, 3),
    new(6.02268356699901081250e-01D, 4),
    new(7.3159430981099115000e-01D, 2),
    new(6.5363542893241332500e-01D, 3),
    new(6.8198497760900255000e-01D, 2),
    new(9.1265404584042750000e-01D, 1),
    new(-5.4287029803597508250e-01D, 4)
}.slice();

internal static slice<float64> gamma = new float64[]{
    2.3254348370739963835386613898e+01D,
    2.991153837155317076427529816e+03D,
    -4.561154336726758060575129109e+00D,
    7.719403468842639065959210984e-01D,
    1.6111876618855418534325755566e+05D,
    1.8706575145216421164173224946e+00D,
    3.4082787447257502836734201635e+01D,
    1.579733951448952054898583387e+00D,
    9.3834586598354592860187267089e-01D,
    -2.093995902923148389186189429e-05D
}.slice();

internal static slice<float64> j0 = new float64[]{
    -1.8444682230601672018219338e-01D,
    2.27353668906331975435892e-01D,
    9.809259936157051116270273e-01D,
    -1.741170131426226587841181e-01D,
    -2.1389448451144143352039069e-01D,
    -2.340905848928038763337414e-01D,
    -1.0029099691890912094586326e-01D,
    -1.5466726714884328135358907e-01D,
    3.252650187653420388714693e-01D,
    -8.72218484409407250005360235e-03D
}.slice();

internal static slice<float64> j1 = new float64[]{
    -3.251526395295203422162967e-01D,
    1.893581711430515718062564e-01D,
    -1.3711761352467242914491514e-01D,
    3.287486536269617297529617e-01D,
    1.3133899188830978473849215e-01D,
    3.660243417832986825301766e-01D,
    -3.4436769271848174665420672e-01D,
    4.329481396640773768835036e-01D,
    5.8181350531954794639333955e-01D,
    -2.7030574577733036112996607e-01D
}.slice();

internal static slice<float64> j2 = new float64[]{
    5.3837518920137802565192769e-02D,
    -1.7841678003393207281244667e-01D,
    9.521746934916464142495821e-03D,
    4.28958355470987397983072e-02D,
    2.4115371837854494725492872e-01D,
    4.842458532394520316844449e-01D,
    -3.142145220618633390125946e-02D,
    4.720849184745124761189957e-01D,
    3.122312022520957042957497e-01D,
    7.096213118930231185707277e-02D
}.slice();

internal static slice<float64> jM3 = new float64[]{
    -3.684042080996403091021151e-01D,
    2.8157665936340887268092661e-01D,
    4.401005480841948348343589e-04D,
    3.629926999056814081597135e-01D,
    3.123672198825455192489266e-02D,
    -2.958805510589623607540455e-01D,
    -3.2033177696533233403289416e-01D,
    -2.592737332129663376736604e-01D,
    -1.0241334641061485092351251e-01D,
    -2.3762660886100206491674503e-01D
}.slice();

internal static slice<fi> lgamma = new fi[]{
    new(3.146492141244545774319734e+00D, 1),
    new(8.003414490659126375852113e+00D, 1),
    new(1.517575735509779707488106e+00D, -1),
    new(-2.588480028182145853558748e-01D, 1),
    new(1.1989897050205555002007985e+01D, 1),
    new(6.262899811091257519386906e-01D, 1),
    new(3.5287924899091566764846037e+00D, 1),
    new(4.5725644770161182299423372e-01D, 1),
    new(-6.363667087767961257654854e-02D, 1),
    new(-1.077385130910300066425564e+01D, -1)
}.slice();

internal static slice<float64> log = new float64[]{
    1.605231462693062999102599e+00D,
    2.0462560018708770653153909e+00D,
    -1.2841708730962657801275038e+00D,
    1.6115563905281545116286206e+00D,
    2.2655365644872016636317461e+00D,
    1.0737652208918379856272735e+00D,
    1.6542360106073546632707956e+00D,
    1.0035467127723465801264487e+00D,
    6.0174879014578057187016475e-01D,
    2.161703872847352815363655e+00D
}.slice();

internal static slice<float64> logb = new float64[]{
    2.0000000000000000e+00D,
    2.0000000000000000e+00D,
    -2.0000000000000000e+00D,
    2.0000000000000000e+00D,
    3.0000000000000000e+00D,
    1.0000000000000000e+00D,
    2.0000000000000000e+00D,
    1.0000000000000000e+00D,
    0.0000000000000000e+00D,
    3.0000000000000000e+00D
}.slice();

internal static slice<float64> log10 = new float64[]{
    6.9714316642508290997617083e-01D,
    8.886776901739320576279124e-01D,
    -5.5770832400658929815908236e-01D,
    6.998900476822994346229723e-01D,
    9.8391002850684232013281033e-01D,
    4.6633031029295153334285302e-01D,
    7.1842557117242328821552533e-01D,
    4.3583479968917773161304553e-01D,
    2.6133617905227038228626834e-01D,
    9.3881606348649405716214241e-01D
}.slice();

internal static slice<float64> log1p = new float64[]{
    4.8590257759797794104158205e-02D,
    7.4540265965225865330849141e-02D,
    -2.7726407903942672823234024e-03D,
    -5.1404917651627649094953380e-02D,
    9.1998280672258624681335010e-02D,
    2.8843762576593352865894824e-02D,
    5.0969534581863707268992645e-02D,
    2.6913947602193238458458594e-02D,
    1.8088493239630770262045333e-02D,
    -9.0865245631588989681559268e-02D
}.slice();

internal static slice<float64> log2 = new float64[]{
    2.3158594707062190618898251e+00D,
    2.9521233862883917703341018e+00D,
    -1.8526669502700329984917062e+00D,
    2.3249844127278861543568029e+00D,
    3.268478366538305087466309e+00D,
    1.5491157592596970278166492e+00D,
    2.3865580889631732407886495e+00D,
    1.447811865817085365540347e+00D,
    8.6813999540425116282815557e-01D,
    3.118679457227342224364709e+00D
}.slice();

internal static slice<array<float64>> modf = GoReflect.WithElemDims(new array<float64>[]{
    new float64[]{4.0000000000000000e+00D, 9.7901192488367350108546816e-01D}.array(),
    new float64[]{7.0000000000000000e+00D, 7.3887247457810456552351752e-01D}.array(),
    new float64[]{Copysign(0D, -1D), -2.7688005719200159404635997e-01D}.array(),
    new float64[]{-5.0000000000000000e+00D, -1.060361827107492160848778e-02D}.array(),
    new float64[]{9.0000000000000000e+00D, 6.3629370719841737980004837e-01D}.array(),
    new float64[]{2.0000000000000000e+00D, 9.2637723924396464525443662e-01D}.array(),
    new float64[]{5.0000000000000000e+00D, 2.2908343145930665230025625e-01D}.array(),
    new float64[]{2.0000000000000000e+00D, 7.2793991043601025126008608e-01D}.array(),
    new float64[]{1.0000000000000000e+00D, 8.2530809168085506044576505e-01D}.array(),
    new float64[]{-8.0000000000000000e+00D, -6.8592476857560136238589621e-01D}.array()
}.slice(), 2);

internal static slice<float32> nextafter32 = new float32[]{
    4.979012489318848e+00F,
    7.738873004913330e+00F,
    -2.768800258636475e-01F,
    -5.010602951049805e+00F,
    9.636294364929199e+00F,
    2.926377534866333e+00F,
    5.229084014892578e+00F,
    2.727940082550049e+00F,
    1.825308203697205e+00F,
    -8.685923576354980e+00F
}.slice();

internal static slice<float64> nextafter64 = new float64[]{
    4.97901192488367438926388786e+00D,
    7.73887247457810545370193722e+00D,
    -2.7688005719200153853520874e-01D,
    -5.01060361827107403343006808e+00D,
    9.63629370719841915615688777e+00D,
    2.92637723924396508934364647e+00D,
    5.22908343145930754047867595e+00D,
    2.72793991043601069534929593e+00D,
    1.82530809168085528249036997e+00D,
    -8.68592476857559958602905681e+00D
}.slice();

internal static slice<float64> pow = new float64[]{
    9.5282232631648411840742957e+04D,
    5.4811599352999901232411871e+07D,
    5.2859121715894396531132279e-01D,
    9.7587991957286474464259698e-06D,
    4.328064329346044846740467e+09D,
    8.4406761805034547437659092e+02D,
    1.6946633276191194947742146e+05D,
    5.3449040147551939075312879e+02D,
    6.688182138451414936380374e+01D,
    2.0609869004248742886827439e-09D
}.slice();

internal static slice<float64> remainder = new float64[]{
    4.197615023265299782906368e-02D,
    2.261127525421895434476482e+00D,
    3.231794108794261433104108e-02D,
    -2.120723654214984321697556e-02D,
    3.637062928015826201999516e-01D,
    1.220868282268106064236690e+00D,
    -4.581668629186133046005125e-01D,
    -9.117596417440410050403443e-01D,
    8.734595415957246977711748e-01D,
    1.314075231424398637614104e+00D
}.slice();

internal static slice<float64> round = new float64[]{
    5D,
    8D,
    Copysign(0D, -1D),
    -5D,
    10D,
    3D,
    5D,
    3D,
    2D,
    -9D
}.slice();

internal static slice<bool> signbit = new bool[]{
    false,
    false,
    true,
    true,
    false,
    false,
    false,
    false,
    false,
    true
}.slice();

internal static slice<float64> sin = new float64[]{
    -9.6466616586009283766724726e-01D,
    9.9338225271646545763467022e-01D,
    -2.7335587039794393342449301e-01D,
    9.5586257685042792878173752e-01D,
    -2.099421066779969164496634e-01D,
    2.135578780799860532750616e-01D,
    -8.694568971167362743327708e-01D,
    4.019566681155577786649878e-01D,
    9.6778633541687993721617774e-01D,
    -6.734405869050344734943028e-01D
}.slice();

// Results for 100000 * Pi + vf[i]
internal static slice<float64> sinLarge = new float64[]{
    -9.646661658548936063912e-01D,
    9.933822527198506903752e-01D,
    -2.7335587036246899796e-01D,
    9.55862576853689321268e-01D,
    -2.099421066862688873691e-01D,
    2.13557878070308981163e-01D,
    -8.694568970959221300497e-01D,
    4.01956668098863248917e-01D,
    9.67786335404528727927e-01D,
    -6.7344058693131973066e-01D
}.slice();

internal static slice<float64> sinh = new float64[]{
    7.2661916084208532301448439e+01D,
    1.1479409110035194500526446e+03D,
    -2.8043136512812518927312641e-01D,
    -7.499429091181587232835164e+01D,
    7.6552466042906758523925934e+03D,
    9.3031583421672014313789064e+00D,
    9.330815755828109072810322e+01D,
    7.6179893137269146407361477e+00D,
    3.021769180549615819524392e+00D,
    -2.95950575724449499189888e+03D
}.slice();

internal static slice<float64> sqrt = new float64[]{
    2.2313699659365484748756904e+00D,
    2.7818829009464263511285458e+00D,
    5.2619393496314796848143251e-01D,
    2.2384377628763938724244104e+00D,
    3.1042380236055381099288487e+00D,
    1.7106657298385224403917771e+00D,
    2.286718922705479046148059e+00D,
    1.6516476350711159636222979e+00D,
    1.3510396336454586262419247e+00D,
    2.9471892997524949215723329e+00D
}.slice();

internal static slice<float64> tan = new float64[]{
    -3.661316565040227801781974e+00D,
    8.64900232648597589369854e+00D,
    -2.8417941955033612725238097e-01D,
    3.253290185974728640827156e+00D,
    2.147275640380293804770778e-01D,
    -2.18600910711067004921551e-01D,
    -1.760002817872367935518928e+00D,
    -4.389808914752818126249079e-01D,
    -3.843885560201130679995041e+00D,
    9.10988793377685105753416e-01D
}.slice();

// Results for 100000 * Pi + vf[i]
internal static slice<float64> tanLarge = new float64[]{
    -3.66131656475596512705e+00D,
    8.6490023287202547927e+00D,
    -2.841794195104782406e-01D,
    3.2532901861033120983e+00D,
    2.14727564046880001365e-01D,
    -2.18600910700688062874e-01D,
    -1.760002817699722747043e+00D,
    -4.38980891453536115952e-01D,
    -3.84388555942723509071e+00D,
    9.1098879344275101051e-01D
}.slice();

internal static slice<float64> tanh = new float64[]{
    9.9990531206936338549262119e-01D,
    9.9999962057085294197613294e-01D,
    -2.7001505097318677233756845e-01D,
    -9.9991110943061718603541401e-01D,
    9.9999999146798465745022007e-01D,
    9.9427249436125236705001048e-01D,
    9.9994257600983138572705076e-01D,
    9.9149409509772875982054701e-01D,
    9.4936501296239685514466577e-01D,
    -9.9999994291374030946055701e-01D
}.slice();

internal static slice<float64> trunc = new float64[]{
    4.0000000000000000e+00D,
    7.0000000000000000e+00D,
    Copysign(0D, -1D),
    -5.0000000000000000e+00D,
    9.0000000000000000e+00D,
    2.0000000000000000e+00D,
    5.0000000000000000e+00D,
    2.0000000000000000e+00D,
    1.0000000000000000e+00D,
    -8.0000000000000000e+00D
}.slice();

internal static slice<float64> y0 = new float64[]{
    -3.053399153780788357534855e-01D,
    1.7437227649515231515503649e-01D,
    -8.6221781263678836910392572e-01D,
    -3.100664880987498407872839e-01D,
    1.422200649300982280645377e-01D,
    4.000004067997901144239363e-01D,
    -3.3340749753099352392332536e-01D,
    4.5399790746668954555205502e-01D,
    4.8290004112497761007536522e-01D,
    2.7036697826604756229601611e-01D
}.slice();

internal static slice<float64> y1 = new float64[]{
    0.15494213737457922210218611D,
    -0.2165955142081145245075746D,
    -2.4644949631241895201032829D,
    0.1442740489541836405154505D,
    0.2215379960518984777080163D,
    0.3038800915160754150565448D,
    0.0691107642452362383808547D,
    0.2380116417809914424860165D,
    -0.20849492979459761009678934D,
    0.0242503179793232308250804D
}.slice();

internal static slice<float64> y2 = new float64[]{
    0.3675780219390303613394936D,
    -0.23034826393250119879267257D,
    -16.939677983817727205631397D,
    0.367653980523052152867791D,
    -0.0962401471767804440353136D,
    -0.1923169356184851105200523D,
    0.35984072054267882391843766D,
    -0.2794987252299739821654982D,
    -0.7113490692587462579757954D,
    -0.2647831587821263302087457D
}.slice();

internal static slice<float64> yM3 = new float64[]{
    -0.14035984421094849100895341D,
    -0.097535139617792072703973D,
    242.25775994555580176377379D,
    -0.1492267014802818619511046D,
    0.26148702629155918694500469D,
    0.56675383593895176530394248D,
    -0.206150264009006981070575D,
    0.64784284687568332737963658D,
    1.3503631555901938037008443D,
    0.1461869756579956803341844D
}.slice();

// arguments and expected results for special cases
internal static slice<float64> vfacosSC = new float64[]{
    -Pi,
    1D,
    Pi,
    NaN()
}.slice();

internal static slice<float64> acosSC = new float64[]{
    NaN(),
    0D,
    NaN(),
    NaN()
}.slice();

internal static slice<float64> vfacoshSC = new float64[]{
    Inf(-1),
    0.5D,
    1D,
    Inf(1),
    NaN()
}.slice();

internal static slice<float64> acoshSC = new float64[]{
    NaN(),
    NaN(),
    0D,
    Inf(1),
    NaN()
}.slice();

internal static slice<float64> vfasinSC = new float64[]{
    -Pi,
    Copysign(0D, -1D),
    0D,
    Pi,
    NaN()
}.slice();

internal static slice<float64> asinSC = new float64[]{
    NaN(),
    Copysign(0D, -1D),
    0D,
    NaN(),
    NaN()
}.slice();

internal static slice<float64> vfasinhSC = new float64[]{
    Inf(-1),
    Copysign(0D, -1D),
    0D,
    Inf(1),
    NaN()
}.slice();

internal static slice<float64> asinhSC = new float64[]{
    Inf(-1),
    Copysign(0D, -1D),
    0D,
    Inf(1),
    NaN()
}.slice();

internal static slice<float64> vfatanSC = new float64[]{
    Inf(-1),
    Copysign(0D, -1D),
    0D,
    Inf(1),
    NaN()
}.slice();

internal static slice<float64> atanSC = new float64[]{
    -Pi / 2D,
    Copysign(0D, -1D),
    0D,
    Pi / 2D,
    NaN()
}.slice();

internal static slice<float64> vfatanhSC = new float64[]{
    Inf(-1),
    -Pi,
    -1D,
    Copysign(0D, -1D),
    0D,
    1D,
    Pi,
    Inf(1),
    NaN()
}.slice();

internal static slice<float64> atanhSC = new float64[]{
    NaN(),
    NaN(),
    Inf(-1),
    Copysign(0D, -1D),
    0D,
    Inf(1),
    NaN(),
    NaN(),
    NaN()
}.slice();

internal static slice<array<float64>> vfatan2SC = GoReflect.WithElemDims(new array<float64>[]{
    new float64[]{Inf(-1), Inf(-1)}.array(),
    new float64[]{Inf(-1), -Pi}.array(),
    new float64[]{Inf(-1), 0D}.array(),
    new float64[]{Inf(-1), +Pi}.array(),
    new float64[]{Inf(-1), Inf(1)}.array(),
    new float64[]{Inf(-1), NaN()}.array(),
    new float64[]{-Pi, Inf(-1)}.array(),
    new float64[]{-Pi, 0D}.array(),
    new float64[]{-Pi, Inf(1)}.array(),
    new float64[]{-Pi, NaN()}.array(),
    new float64[]{Copysign(0D, -1D), Inf(-1)}.array(),
    new float64[]{Copysign(0D, -1D), -Pi}.array(),
    new float64[]{Copysign(0D, -1D), Copysign(0D, -1D)}.array(),
    new float64[]{Copysign(0D, -1D), 0D}.array(),
    new float64[]{Copysign(0D, -1D), +Pi}.array(),
    new float64[]{Copysign(0D, -1D), Inf(1)}.array(),
    new float64[]{Copysign(0D, -1D), NaN()}.array(),
    new float64[]{0D, Inf(-1)}.array(),
    new float64[]{0D, -Pi}.array(),
    new float64[]{0D, Copysign(0D, -1D)}.array(),
    new float64[]{0D, 0D}.array(),
    new float64[]{0D, +Pi}.array(),
    new float64[]{0D, Inf(1)}.array(),
    new float64[]{0D, NaN()}.array(),
    new float64[]{+Pi, Inf(-1)}.array(),
    new float64[]{+Pi, 0D}.array(),
    new float64[]{+Pi, Inf(1)}.array(),
    new float64[]{1.0D, Inf(1)}.array(),
    new float64[]{-1.0D, Inf(1)}.array(),
    new float64[]{+Pi, NaN()}.array(),
    new float64[]{Inf(1), Inf(-1)}.array(),
    new float64[]{Inf(1), -Pi}.array(),
    new float64[]{Inf(1), 0D}.array(),
    new float64[]{Inf(1), +Pi}.array(),
    new float64[]{Inf(1), Inf(1)}.array(),
    new float64[]{Inf(1), NaN()}.array(),
    new float64[]{NaN(), NaN()}.array()
}.slice(), 2);

// atan2(-Inf, -Inf)
// atan2(-Inf, -Pi)
// atan2(-Inf, +0)
// atan2(-Inf, +Pi)
// atan2(-Inf, +Inf)
// atan2(-Inf, NaN)
// atan2(-Pi, -Inf)
// atan2(-Pi, +0)
// atan2(-Pi, Inf)
// atan2(-Pi, NaN)
// atan2(-0, -Inf)
// atan2(-0, -Pi)
// atan2(-0, -0)
// atan2(-0, +0)
// atan2(-0, +Pi)
// atan2(-0, +Inf)
// atan2(-0, NaN)
// atan2(+0, -Inf)
// atan2(+0, -Pi)
// atan2(+0, -0)
// atan2(+0, +0)
// atan2(+0, +Pi)
// atan2(+0, +Inf)
// atan2(+0, NaN)
// atan2(+Pi, -Inf)
// atan2(+Pi, +0)
// atan2(+Pi, +Inf)
// atan2(+1, +Inf)
// atan2(-1, +Inf)
// atan2(+Pi, NaN)
// atan2(+Inf, -Inf)
// atan2(+Inf, -Pi)
// atan2(+Inf, +0)
// atan2(+Inf, +Pi)
// atan2(+Inf, +Inf)
// atan2(+Inf, NaN)
// atan2(NaN, NaN)
internal static slice<float64> atan2SC = new float64[]{
    -3D * Pi / 4D,
    -Pi / 2D,
    -Pi / 2D,
    -Pi / 2D,
    -Pi / 4D,
    NaN(),
    -Pi,
    -Pi / 2D,
    Copysign(0D, -1D),
    NaN(),
    -Pi,
    -Pi,
    -Pi,
    Copysign(0D, -1D),
    Copysign(0D, -1D),
    Copysign(0D, -1D),
    NaN(),
    Pi,
    Pi,
    Pi,
    0D,
    0D,
    0D,
    NaN(),
    Pi,
    Pi / 2D,
    0D,
    0D,
    Copysign(0D, -1D),
    NaN(),
    3D * Pi / 4D,
    Pi / 2D,
    Pi / 2D,
    Pi / 2D,
    Pi / 4D,
    NaN(),
    NaN()
}.slice();

internal static slice<float64> vfcbrtSC = new float64[]{
    Inf(-1),
    Copysign(0D, -1D),
    0D,
    Inf(1),
    NaN()
}.slice();

internal static slice<float64> cbrtSC = new float64[]{
    Inf(-1),
    Copysign(0D, -1D),
    0D,
    Inf(1),
    NaN()
}.slice();

// largest fractional float64
// smallest fractional float64
internal static slice<float64> vfceilSC = new float64[]{
    Inf(-1),
    Copysign(0D, -1D),
    0D,
    Inf(1),
    NaN(),
    4503599627370495D,
    4503599627370496D - 0.5D,
    4503599627370496D,
    -4503599627370496D,
    -4503599627370496D + 0.5D,
    -4503599627370495D,
    9007199254740992D,
    -9007199254740992D
}.slice();

internal static slice<float64> ceilBaseSC = new float64[]{
    Inf(-1),
    Copysign(0D, -1D),
    0D,
    Inf(1),
    NaN()
}.slice();

internal static slice<float64> ceilSC = append(ceilBaseSC,
    4503599627370495D,
    4503599627370496D,
    4503599627370496D,
    -4503599627370496D,
    -4503599627370495D,
    -4503599627370495D,
    9007199254740992D,
    -9007199254740992D);

internal static slice<float64> floorSC = append(ceilBaseSC,
    4503599627370495D,
    4503599627370495D,
    4503599627370496D,
    -4503599627370496D,
    -4503599627370496D,
    -4503599627370495D,
    9007199254740992D,
    -9007199254740992D);

internal static slice<float64> truncSC = append(ceilBaseSC,
    4503599627370495D,
    4503599627370495D,
    4503599627370496D,
    -4503599627370496D,
    -4503599627370495D,
    -4503599627370495D,
    9007199254740992D,
    -9007199254740992D);

internal static slice<float64> vfcopysignSC = new float64[]{
    Inf(-1),
    Inf(1),
    NaN()
}.slice();

internal static slice<float64> copysignSC = new float64[]{
    Inf(-1),
    Inf(-1),
    NaN()
}.slice();

internal static slice<float64> vfcosSC = new float64[]{
    Inf(-1),
    Inf(1),
    NaN()
}.slice();

internal static slice<float64> cosSC = new float64[]{
    NaN(),
    NaN(),
    NaN()
}.slice();

internal static slice<float64> vfcoshSC = new float64[]{
    Inf(-1),
    Copysign(0D, -1D),
    0D,
    Inf(1),
    NaN()
}.slice();

internal static slice<float64> coshSC = new float64[]{
    Inf(1),
    1D,
    1D,
    Inf(1),
    NaN()
}.slice();

internal static slice<float64> vferfSC = new float64[]{
    Inf(-1),
    Copysign(0D, -1D),
    0D,
    Inf(1),
    NaN(),
    -1000D,
    1000D
}.slice();

internal static slice<float64> erfSC = new float64[]{
    -1D,
    Copysign(0D, -1D),
    0D,
    1D,
    NaN(),
    -1D,
    1D
}.slice();

internal static slice<float64> vferfcSC = new float64[]{
    Inf(-1),
    Inf(1),
    NaN(),
    -1000D,
    1000D
}.slice();

internal static slice<float64> erfcSC = new float64[]{
    2D,
    0D,
    NaN(),
    2D,
    0D
}.slice();

internal static slice<float64> vferfinvSC = new float64[]{
    1D,
    -1D,
    0D,
    Inf(-1),
    Inf(1),
    NaN()
}.slice();

internal static slice<float64> erfinvSC = new float64[]{
    Inf(+1),
    Inf(-1),
    0D,
    NaN(),
    NaN(),
    NaN()
}.slice();

internal static slice<float64> vferfcinvSC = new float64[]{
    0D,
    2D,
    1D,
    Inf(1),
    Inf(-1),
    NaN()
}.slice();

internal static slice<float64> erfcinvSC = new float64[]{
    Inf(+1),
    Inf(-1),
    0D,
    NaN(),
    NaN(),
    NaN()
}.slice();

// smallest float64 that overflows Exp(x)
// Issue 18912
// near zero
// denormal
internal static slice<float64> vfexpSC = new float64[]{
    Inf(-1),
    -2000D,
    2000D,
    Inf(1),
    NaN(),
    7.097827128933841e+02D,
    1.48852223e+09D,
    1.4885222e+09D,
    1D,
    3.725290298461915e-09D,
    -740D
}.slice();

internal static slice<float64> expSC = new float64[]{
    0D,
    0D,
    Inf(1),
    Inf(1),
    NaN(),
    Inf(1),
    Inf(1),
    Inf(1),
    2.718281828459045D,
    1.0000000037252903D,
    4.2e-322D
}.slice();

// smallest float64 that overflows Exp2(x)
// near underflow
// near zero
internal static slice<float64> vfexp2SC = new float64[]{
    Inf(-1),
    -2000D,
    2000D,
    Inf(1),
    NaN(),
    1024D,
    -1.07399999999999e+03D,
    3.725290298461915e-09D
}.slice();

internal static slice<float64> exp2SC = new float64[]{
    0D,
    0D,
    Inf(1),
    Inf(1),
    NaN(),
    Inf(1),
    5e-324D,
    1.0000000025821745D
}.slice();

internal static slice<float64> vfexpm1SC = new float64[]{
    Inf(-1),
    -710D,
    Copysign(0D, -1D),
    0D,
    710D,
    Inf(1),
    NaN()
}.slice();

internal static slice<float64> expm1SC = new float64[]{
    -1D,
    -1D,
    Copysign(0D, -1D),
    0D,
    Inf(1),
    Inf(1),
    NaN()
}.slice();

internal static slice<float64> vffabsSC = new float64[]{
    Inf(-1),
    Copysign(0D, -1D),
    0D,
    Inf(1),
    NaN()
}.slice();

internal static slice<float64> fabsSC = new float64[]{
    Inf(1),
    0D,
    0D,
    Inf(1),
    NaN()
}.slice();

internal static slice<array<float64>> vffdimSC = GoReflect.WithElemDims(new array<float64>[]{
    new float64[]{Inf(-1), Inf(-1)}.array(),
    new float64[]{Inf(-1), Inf(1)}.array(),
    new float64[]{Inf(-1), NaN()}.array(),
    new float64[]{Copysign(0D, -1D), Copysign(0D, -1D)}.array(),
    new float64[]{Copysign(0D, -1D), 0D}.array(),
    new float64[]{0D, Copysign(0D, -1D)}.array(),
    new float64[]{0D, 0D}.array(),
    new float64[]{Inf(1), Inf(-1)}.array(),
    new float64[]{Inf(1), Inf(1)}.array(),
    new float64[]{Inf(1), NaN()}.array(),
    new float64[]{NaN(), Inf(-1)}.array(),
    new float64[]{NaN(), Copysign(0D, -1D)}.array(),
    new float64[]{NaN(), 0D}.array(),
    new float64[]{NaN(), Inf(1)}.array(),
    new float64[]{NaN(), NaN()}.array()
}.slice(), 2);

internal static float64 nan = Float64frombits(0xFFF8000000000000UL); // SSE2 DIVSD 0/0

internal static slice<array<float64>> vffdim2SC = GoReflect.WithElemDims(new array<float64>[]{
    new float64[]{Inf(-1), Inf(-1)}.array(),
    new float64[]{Inf(-1), Inf(1)}.array(),
    new float64[]{Inf(-1), nan}.array(),
    new float64[]{Copysign(0D, -1D), Copysign(0D, -1D)}.array(),
    new float64[]{Copysign(0D, -1D), 0D}.array(),
    new float64[]{0D, Copysign(0D, -1D)}.array(),
    new float64[]{0D, 0D}.array(),
    new float64[]{Inf(1), Inf(-1)}.array(),
    new float64[]{Inf(1), Inf(1)}.array(),
    new float64[]{Inf(1), nan}.array(),
    new float64[]{nan, Inf(-1)}.array(),
    new float64[]{nan, Copysign(0D, -1D)}.array(),
    new float64[]{nan, 0D}.array(),
    new float64[]{nan, Inf(1)}.array(),
    new float64[]{nan, nan}.array()
}.slice(), 2);

internal static slice<float64> fdimSC = new float64[]{
    NaN(),
    0D,
    NaN(),
    0D,
    0D,
    0D,
    0D,
    Inf(1),
    NaN(),
    NaN(),
    NaN(),
    NaN(),
    NaN(),
    NaN(),
    NaN()
}.slice();

internal static slice<float64> fmaxSC = new float64[]{
    Inf(-1),
    Inf(1),
    NaN(),
    Copysign(0D, -1D),
    0D,
    0D,
    0D,
    Inf(1),
    Inf(1),
    Inf(1),
    NaN(),
    NaN(),
    NaN(),
    Inf(1),
    NaN()
}.slice();

internal static slice<float64> fminSC = new float64[]{
    Inf(-1),
    Inf(-1),
    Inf(-1),
    Copysign(0D, -1D),
    Copysign(0D, -1D),
    Copysign(0D, -1D),
    0D,
    Inf(-1),
    Inf(1),
    NaN(),
    Inf(-1),
    NaN(),
    NaN(),
    NaN(),
    NaN()
}.slice();

internal static slice<array<float64>> vffmodSC = GoReflect.WithElemDims(new array<float64>[]{
    new float64[]{Inf(-1), Inf(-1)}.array(),
    new float64[]{Inf(-1), -Pi}.array(),
    new float64[]{Inf(-1), 0D}.array(),
    new float64[]{Inf(-1), Pi}.array(),
    new float64[]{Inf(-1), Inf(1)}.array(),
    new float64[]{Inf(-1), NaN()}.array(),
    new float64[]{-Pi, Inf(-1)}.array(),
    new float64[]{-Pi, 0D}.array(),
    new float64[]{-Pi, Inf(1)}.array(),
    new float64[]{-Pi, NaN()}.array(),
    new float64[]{Copysign(0D, -1D), Inf(-1)}.array(),
    new float64[]{Copysign(0D, -1D), 0D}.array(),
    new float64[]{Copysign(0D, -1D), Inf(1)}.array(),
    new float64[]{Copysign(0D, -1D), NaN()}.array(),
    new float64[]{0D, Inf(-1)}.array(),
    new float64[]{0D, 0D}.array(),
    new float64[]{0D, Inf(1)}.array(),
    new float64[]{0D, NaN()}.array(),
    new float64[]{Pi, Inf(-1)}.array(),
    new float64[]{Pi, 0D}.array(),
    new float64[]{Pi, Inf(1)}.array(),
    new float64[]{Pi, NaN()}.array(),
    new float64[]{Inf(1), Inf(-1)}.array(),
    new float64[]{Inf(1), -Pi}.array(),
    new float64[]{Inf(1), 0D}.array(),
    new float64[]{Inf(1), Pi}.array(),
    new float64[]{Inf(1), Inf(1)}.array(),
    new float64[]{Inf(1), NaN()}.array(),
    new float64[]{NaN(), Inf(-1)}.array(),
    new float64[]{NaN(), -Pi}.array(),
    new float64[]{NaN(), 0D}.array(),
    new float64[]{NaN(), Pi}.array(),
    new float64[]{NaN(), Inf(1)}.array(),
    new float64[]{NaN(), NaN()}.array()
}.slice(), 2);

// fmod(-Inf, -Inf)
// fmod(-Inf, -Pi)
// fmod(-Inf, 0)
// fmod(-Inf, Pi)
// fmod(-Inf, +Inf)
// fmod(-Inf, NaN)
// fmod(-Pi, -Inf)
// fmod(-Pi, 0)
// fmod(-Pi, +Inf)
// fmod(-Pi, NaN)
// fmod(-0, -Inf)
// fmod(-0, 0)
// fmod(-0, Inf)
// fmod(-0, NaN)
// fmod(0, -Inf)
// fmod(0, 0)
// fmod(0, +Inf)
// fmod(0, NaN)
// fmod(Pi, -Inf)
// fmod(Pi, 0)
// fmod(Pi, +Inf)
// fmod(Pi, NaN)
// fmod(+Inf, -Inf)
// fmod(+Inf, -Pi)
// fmod(+Inf, 0)
// fmod(+Inf, Pi)
// fmod(+Inf, +Inf)
// fmod(+Inf, NaN)
// fmod(NaN, -Inf)
// fmod(NaN, -Pi)
// fmod(NaN, 0)
// fmod(NaN, Pi)
// fmod(NaN, +Inf)
// fmod(NaN, NaN)
internal static slice<float64> fmodSC = new float64[]{
    NaN(),
    NaN(),
    NaN(),
    NaN(),
    NaN(),
    NaN(),
    -Pi,
    NaN(),
    -Pi,
    NaN(),
    Copysign(0D, -1D),
    NaN(),
    Copysign(0D, -1D),
    NaN(),
    0D,
    NaN(),
    0D,
    NaN(),
    Pi,
    NaN(),
    Pi,
    NaN(),
    NaN(),
    NaN(),
    NaN(),
    NaN(),
    NaN(),
    NaN(),
    NaN(),
    NaN(),
    NaN(),
    NaN(),
    NaN(),
    NaN()
}.slice();

internal static slice<float64> vffrexpSC = new float64[]{
    Inf(-1),
    Copysign(0D, -1D),
    0D,
    Inf(1),
    NaN()
}.slice();

internal static slice<fi> frexpSC = new fi[]{
    new(Inf(-1), 0),
    new(Copysign(0D, -1D), 0),
    new(0D, 0),
    new(Inf(1), 0),
    new(NaN(), 0)
}.slice();

// Test inputs inspired by Python test suite.
// Outputs computed at high precision by PARI/GP.
// If recomputing table entries, be careful to use
// high-precision (%.1000g) formatting of the float64 inputs.
// For example, -2.0000000000000004 is the float64 with exact value
// -2.00000000000000044408920985626161695, and
// gamma(-2.0000000000000004) = -1249999999999999.5386078562728167651513, while
// gamma(-2.00000000000000044408920985626161695) = -1125899906826907.2044875028130093136826.
// Thus the table lists -1.1258999068426235e+15 as the answer.
internal static slice<array<float64>> vfgamma = GoReflect.WithElemDims(new array<float64>[]{
    new float64[]{Inf(1), Inf(1)}.array(),
    new float64[]{Inf(-1), NaN()}.array(),
    new float64[]{0D, Inf(1)}.array(),
    new float64[]{Copysign(0D, -1D), Inf(-1)}.array(),
    new float64[]{NaN(), NaN()}.array(),
    new float64[]{-1D, NaN()}.array(),
    new float64[]{-2D, NaN()}.array(),
    new float64[]{-3D, NaN()}.array(),
    new float64[]{-1e16D, NaN()}.array(),
    new float64[]{-1e300D, NaN()}.array(),
    new float64[]{1.7e308D, Inf(1)}.array(),
    new float64[]{0.5D, 1.772453850905516D}.array(),
    new float64[]{1.5D, 0.886226925452758D}.array(),
    new float64[]{2.5D, 1.329340388179137D}.array(),
    new float64[]{3.5D, 3.3233509704478426D}.array(),
    new float64[]{-0.5D, -3.544907701811032D}.array(),
    new float64[]{-1.5D, 2.363271801207355D}.array(),
    new float64[]{-2.5D, -0.9453087204829419D}.array(),
    new float64[]{-3.5D, 0.2700882058522691D}.array(),
    new float64[]{0.1D, 9.51350769866873D}.array(),
    new float64[]{0.01D, 99.4325851191506D}.array(),
    new float64[]{1e-08D, 9.999999942278434e+07D}.array(),
    new float64[]{1e-16D, 1e+16D}.array(),
    new float64[]{0.001D, 999.4237724845955D}.array(),
    new float64[]{1e-16D, 1e+16D}.array(),
    new float64[]{1e-308D, 1e+308D}.array(),
    new float64[]{5.6e-309D, 1.7857142857142864e+308D}.array(),
    new float64[]{5.5e-309D, Inf(1)}.array(),
    new float64[]{1e-309D, Inf(1)}.array(),
    new float64[]{1e-323D, Inf(1)}.array(),
    new float64[]{5e-324D, Inf(1)}.array(),
    new float64[]{-0.1D, -10.686287021193193D}.array(),
    new float64[]{-0.01D, -100.58719796441078D}.array(),
    new float64[]{-1e-08D, -1.0000000057721567e+08D}.array(),
    new float64[]{-1e-16D, -1e+16D}.array(),
    new float64[]{-0.001D, -1000.5782056293586D}.array(),
    new float64[]{-1e-16D, -1e+16D}.array(),
    new float64[]{-1e-308D, -1e+308D}.array(),
    new float64[]{-5.6e-309D, -1.7857142857142864e+308D}.array(),
    new float64[]{-5.5e-309D, Inf(-1)}.array(),
    new float64[]{-1e-309D, Inf(-1)}.array(),
    new float64[]{-1e-323D, Inf(-1)}.array(),
    new float64[]{-5e-324D, Inf(-1)}.array(),
    new float64[]{-0.9999999999999999D, -9.007199254740992e+15D}.array(),
    new float64[]{-1.0000000000000002D, 4.5035996273704955e+15D}.array(),
    new float64[]{-1.9999999999999998D, 2.2517998136852485e+15D}.array(),
    new float64[]{-2.0000000000000004D, -1.1258999068426235e+15D}.array(),
    new float64[]{-100.00000000000001D, -7.540083334883109e-145D}.array(),
    new float64[]{-99.99999999999999D, 7.540083334884096e-145D}.array(),
    new float64[]{17D, 2.0922789888e+13D}.array(),
    new float64[]{171D, 7.257415615307999e+306D}.array(),
    new float64[]{171.6D, 1.5858969096672565e+308D}.array(),
    new float64[]{171.624D, 1.7942117599248104e+308D}.array(),
    new float64[]{171.625D, Inf(1)}.array(),
    new float64[]{172D, Inf(1)}.array(),
    new float64[]{2000D, Inf(1)}.array(),
    new float64[]{-100.5D, -3.3536908198076787e-159D}.array(),
    new float64[]{-160.5D, -5.255546447007829e-286D}.array(),
    new float64[]{-170.5D, -3.3127395215386074e-308D}.array(),
    new float64[]{-171.5D, 1.9316265431712e-310D}.array(),
    new float64[]{-176.5D, -1.196e-321D}.array(),
    new float64[]{-177.5D, 5e-324D}.array(),
    new float64[]{-178.5D, Copysign(0D, -1D)}.array(),
    new float64[]{-179.5D, 0D}.array(),
    new float64[]{-201.0001D, 0D}.array(),
    new float64[]{-202.9999D, Copysign(0D, -1D)}.array(),
    new float64[]{-1000.5D, Copysign(0D, -1D)}.array(),
    new float64[]{-1.0000000003e+09D, Copysign(0D, -1D)}.array(),
    new float64[]{-4.5035996273704955e+15D, 0D}.array(),
    new float64[]{-63.349078729022985D, 4.177797167776188e-88D}.array(),
    new float64[]{-127.45117632943295D, 1.183111089623681e-214D}.array()
}.slice(), 2);

// +0, +0
internal static slice<array<float64>> vfhypotSC = GoReflect.WithElemDims(new array<float64>[]{
    new float64[]{Inf(-1), Inf(-1)}.array(),
    new float64[]{Inf(-1), 0D}.array(),
    new float64[]{Inf(-1), Inf(1)}.array(),
    new float64[]{Inf(-1), NaN()}.array(),
    new float64[]{Copysign(0D, -1D), Copysign(0D, -1D)}.array(),
    new float64[]{Copysign(0D, -1D), 0D}.array(),
    new float64[]{0D, Copysign(0D, -1D)}.array(),
    new float64[]{0D, 0D}.array(),
    new float64[]{0D, Inf(-1)}.array(),
    new float64[]{0D, Inf(1)}.array(),
    new float64[]{0D, NaN()}.array(),
    new float64[]{Inf(1), Inf(-1)}.array(),
    new float64[]{Inf(1), 0D}.array(),
    new float64[]{Inf(1), Inf(1)}.array(),
    new float64[]{Inf(1), NaN()}.array(),
    new float64[]{NaN(), Inf(-1)}.array(),
    new float64[]{NaN(), 0D}.array(),
    new float64[]{NaN(), Inf(1)}.array(),
    new float64[]{NaN(), NaN()}.array()
}.slice(), 2);

internal static slice<float64> hypotSC = new float64[]{
    Inf(1),
    Inf(1),
    Inf(1),
    Inf(1),
    0D,
    0D,
    0D,
    0D,
    Inf(1),
    Inf(1),
    NaN(),
    Inf(1),
    Inf(1),
    Inf(1),
    Inf(1),
    Inf(1),
    NaN(),
    Inf(1),
    NaN()
}.slice();

internal static slice<nint> ilogbSC = new nint[]{
    MaxInt32,
    MinInt32,
    MaxInt32,
    MaxInt32
}.slice();

internal static slice<float64> vfj0SC = new float64[]{
    Inf(-1),
    0D,
    Inf(1),
    NaN()
}.slice();

internal static slice<float64> j0SC = new float64[]{
    0D,
    1D,
    0D,
    NaN()
}.slice();

internal static slice<float64> j1SC = new float64[]{
    0D,
    0D,
    0D,
    NaN()
}.slice();

internal static slice<float64> j2SC = new float64[]{
    0D,
    0D,
    0D,
    NaN()
}.slice();

internal static slice<float64> jM3SC = new float64[]{
    0D,
    0D,
    0D,
    NaN()
}.slice();

internal static slice<fi> vfldexpSC = new fi[]{
    new(0D, 0),
    new(0D, -1075),
    new(0D, 1024),
    new(Copysign(0D, -1D), 0),
    new(Copysign(0D, -1D), -1075),
    new(Copysign(0D, -1D), 1024),
    new(Inf(1), 0),
    new(Inf(1), -1024),
    new(Inf(-1), 0),
    new(Inf(-1), -1024),
    new(NaN(), -1024),
    new(10D, unchecked((nint)(72057594037927936L))),
    new(10D, (nint)(-(unchecked((nint)(72057594037927936L)))))
}.slice();

internal static slice<float64> ldexpSC = new float64[]{
    0D,
    0D,
    0D,
    Copysign(0D, -1D),
    Copysign(0D, -1D),
    Copysign(0D, -1D),
    Inf(1),
    Inf(1),
    Inf(-1),
    Inf(-1),
    NaN(),
    Inf(1),
    0D
}.slice();

internal static slice<float64> vflgammaSC = new float64[]{
    Inf(-1),
    -3D,
    0D,
    1D,
    2D,
    Inf(1),
    NaN()
}.slice();

internal static slice<fi> lgammaSC = new fi[]{
    new(Inf(-1), 1),
    new(Inf(1), 1),
    new(Inf(1), 1),
    new(0D, 1),
    new(0D, 1),
    new(Inf(1), 1),
    new(NaN(), 1)
}.slice();

internal static slice<float64> vflogSC = new float64[]{
    Inf(-1),
    -Pi,
    Copysign(0D, -1D),
    0D,
    1D,
    Inf(1),
    NaN()
}.slice();

internal static slice<float64> logSC = new float64[]{
    NaN(),
    NaN(),
    Inf(-1),
    Inf(-1),
    0D,
    Inf(1),
    NaN()
}.slice();

internal static slice<float64> vflogbSC = new float64[]{
    Inf(-1),
    0D,
    Inf(1),
    NaN()
}.slice();

internal static slice<float64> logbSC = new float64[]{
    Inf(1),
    Inf(-1),
    Inf(1),
    NaN()
}.slice();

// Issue #29488
internal static slice<float64> vflog1pSC = new float64[]{
    Inf(-1),
    -Pi,
    -1D,
    Copysign(0D, -1D),
    0D,
    Inf(1),
    NaN(),
    4503599627370496.5D
}.slice();

// Issue #29488
internal static slice<float64> log1pSC = new float64[]{
    NaN(),
    NaN(),
    Inf(-1),
    Copysign(0D, -1D),
    0D,
    Inf(1),
    NaN(),
    36.04365338911715D
}.slice();

internal static slice<float64> vfmodfSC = new float64[]{
    Inf(-1),
    Copysign(0D, -1D),
    Inf(1),
    NaN()
}.slice();

// [2]float64{Copysign(0, -1), Inf(-1)},
// [2]float64{0, Inf(1)},
internal static slice<array<float64>> modfSC = GoReflect.WithElemDims(new array<float64>[]{
    new float64[]{Inf(-1), NaN()}.array(),
    new float64[]{Copysign(0D, -1D), Copysign(0D, -1D)}.array(),
    new float64[]{Inf(1), NaN()}.array(),
    new float64[]{NaN(), NaN()}.array()
}.slice(), 2);

internal static slice<array<float32>> vfnextafter32SC = GoReflect.WithElemDims(new array<float32>[]{
    new float32[]{0F, 0F}.array(),
    new float32[]{0F, (float32)Copysign(0D, -1D)}.array(),
    new float32[]{0F, -1F}.array(),
    new float32[]{0F, (float32)NaN()}.array(),
    new float32[]{(float32)Copysign(0D, -1D), 1F}.array(),
    new float32[]{(float32)Copysign(0D, -1D), 0F}.array(),
    new float32[]{(float32)Copysign(0D, -1D), (float32)Copysign(0D, -1D)}.array(),
    new float32[]{(float32)Copysign(0D, -1D), -1F}.array(),
    new float32[]{(float32)NaN(), 0F}.array(),
    new float32[]{(float32)NaN(), (float32)NaN()}.array()
}.slice(), 2);

// Float32frombits(0x80000001)
// Float32frombits(0x00000001)
// Float32frombits(0x80000001)
internal static slice<float32> nextafter32SC = new float32[]{
    0F,
    0F,
    -1.401298464e-45F,
    (float32)NaN(),
    1.401298464e-45F,
    (float32)Copysign(0D, -1D),
    (float32)Copysign(0D, -1D),
    -1.401298464e-45F,
    (float32)NaN(),
    (float32)NaN()
}.slice();

internal static slice<array<float64>> vfnextafter64SC = GoReflect.WithElemDims(new array<float64>[]{
    new float64[]{0D, 0D}.array(),
    new float64[]{0D, Copysign(0D, -1D)}.array(),
    new float64[]{0D, -1D}.array(),
    new float64[]{0D, NaN()}.array(),
    new float64[]{Copysign(0D, -1D), 1D}.array(),
    new float64[]{Copysign(0D, -1D), 0D}.array(),
    new float64[]{Copysign(0D, -1D), Copysign(0D, -1D)}.array(),
    new float64[]{Copysign(0D, -1D), -1D}.array(),
    new float64[]{NaN(), 0D}.array(),
    new float64[]{NaN(), NaN()}.array()
}.slice(), 2);

// Float64frombits(0x8000000000000001)
// Float64frombits(0x0000000000000001)
// Float64frombits(0x8000000000000001)
internal static slice<float64> nextafter64SC = new float64[]{
    0D,
    0D,
    -4.9406564584124654418e-324D,
    NaN(),
    4.9406564584124654418e-324D,
    Copysign(0D, -1D),
    Copysign(0D, -1D),
    -4.9406564584124654418e-324D,
    NaN(),
    NaN()
}.slice();

// Issue #7394 overflow checks
// Issue #57465
internal static slice<array<float64>> vfpowSC = GoReflect.WithElemDims(new array<float64>[]{
    new float64[]{Inf(-1), -Pi}.array(),
    new float64[]{Inf(-1), -3D}.array(),
    new float64[]{Inf(-1), Copysign(0D, -1D)}.array(),
    new float64[]{Inf(-1), 0D}.array(),
    new float64[]{Inf(-1), 1D}.array(),
    new float64[]{Inf(-1), 3D}.array(),
    new float64[]{Inf(-1), Pi}.array(),
    new float64[]{Inf(-1), 0.5D}.array(),
    new float64[]{Inf(-1), NaN()}.array(),
    new float64[]{-Pi, Inf(-1)}.array(),
    new float64[]{-Pi, -Pi}.array(),
    new float64[]{-Pi, Copysign(0D, -1D)}.array(),
    new float64[]{-Pi, 0D}.array(),
    new float64[]{-Pi, 1D}.array(),
    new float64[]{-Pi, Pi}.array(),
    new float64[]{-Pi, Inf(1)}.array(),
    new float64[]{-Pi, NaN()}.array(),
    new float64[]{-1D, Inf(-1)}.array(),
    new float64[]{-1D, Inf(1)}.array(),
    new float64[]{-1D, NaN()}.array(),
    new float64[]{-0.5D, Inf(-1)}.array(),
    new float64[]{-0.5D, Inf(1)}.array(),
    new float64[]{Copysign(0D, -1D), Inf(-1)}.array(),
    new float64[]{Copysign(0D, -1D), -Pi}.array(),
    new float64[]{Copysign(0D, -1D), -0.5D}.array(),
    new float64[]{Copysign(0D, -1D), -3D}.array(),
    new float64[]{Copysign(0D, -1D), 3D}.array(),
    new float64[]{Copysign(0D, -1D), Pi}.array(),
    new float64[]{Copysign(0D, -1D), 0.5D}.array(),
    new float64[]{Copysign(0D, -1D), Inf(1)}.array(),
    new float64[]{0D, Inf(-1)}.array(),
    new float64[]{0D, -Pi}.array(),
    new float64[]{0D, -3D}.array(),
    new float64[]{0D, Copysign(0D, -1D)}.array(),
    new float64[]{0D, 0D}.array(),
    new float64[]{0D, 3D}.array(),
    new float64[]{0D, Pi}.array(),
    new float64[]{0D, Inf(1)}.array(),
    new float64[]{0D, NaN()}.array(),
    new float64[]{0.5D, Inf(-1)}.array(),
    new float64[]{0.5D, Inf(1)}.array(),
    new float64[]{1D, Inf(-1)}.array(),
    new float64[]{1D, Inf(1)}.array(),
    new float64[]{1D, NaN()}.array(),
    new float64[]{Pi, Inf(-1)}.array(),
    new float64[]{Pi, Copysign(0D, -1D)}.array(),
    new float64[]{Pi, 0D}.array(),
    new float64[]{Pi, 1D}.array(),
    new float64[]{Pi, Inf(1)}.array(),
    new float64[]{Pi, NaN()}.array(),
    new float64[]{Inf(1), -Pi}.array(),
    new float64[]{Inf(1), Copysign(0D, -1D)}.array(),
    new float64[]{Inf(1), 0D}.array(),
    new float64[]{Inf(1), 1D}.array(),
    new float64[]{Inf(1), Pi}.array(),
    new float64[]{Inf(1), NaN()}.array(),
    new float64[]{NaN(), -Pi}.array(),
    new float64[]{NaN(), Copysign(0D, -1D)}.array(),
    new float64[]{NaN(), 0D}.array(),
    new float64[]{NaN(), 1D}.array(),
    new float64[]{NaN(), Pi}.array(),
    new float64[]{NaN(), NaN()}.array(),
    new float64[]{2D, (float64)(4294967296D)}.array(),
    new float64[]{2D, -(float64)(4294967296D)}.array(),
    new float64[]{-2D, (float64)(4294967297D)}.array(),
    new float64[]{0.5D, (float64)(35184372088832D)}.array(),
    new float64[]{0.5D, -(float64)(35184372088832D)}.array(),
    new float64[]{Nextafter(1D, 2D), (float64)(9223372036854775808D)}.array(),
    new float64[]{Nextafter(1D, -2D), (float64)(9223372036854775808D)}.array(),
    new float64[]{Nextafter(-1D, 2D), (float64)(9223372036854775808D)}.array(),
    new float64[]{Nextafter(-1D, -2D), (float64)(9223372036854775808D)}.array(),
    new float64[]{Copysign(0D, -1D), 1e19D}.array(),
    new float64[]{Copysign(0D, -1D), -1e19D}.array(),
    new float64[]{Copysign(0D, -1D), 9007199254740991D}.array(),
    new float64[]{Copysign(0D, -1D), -(9007199254740991L)}.array()
}.slice(), 2);

// pow(-Inf, -Pi)
// pow(-Inf, -3)
// pow(-Inf, -0)
// pow(-Inf, +0)
// pow(-Inf, 1)
// pow(-Inf, 3)
// pow(-Inf, Pi)
// pow(-Inf, 0.5)
// pow(-Inf, NaN)
// pow(-Pi, -Inf)
// pow(-Pi, -Pi)
// pow(-Pi, -0)
// pow(-Pi, +0)
// pow(-Pi, 1)
// pow(-Pi, Pi)
// pow(-Pi, +Inf)
// pow(-Pi, NaN)
// pow(-1, -Inf) IEEE 754-2008
// pow(-1, +Inf) IEEE 754-2008
// pow(-1, NaN)
// pow(-1/2, -Inf)
// pow(-1/2, +Inf)
// pow(-0, -Inf)
// pow(-0, -Pi)
// pow(-0, -0.5)
// pow(-0, -3) IEEE 754-2008
// pow(-0, 3) IEEE 754-2008
// pow(-0, +Pi)
// pow(-0, 0.5)
// pow(-0, +Inf)
// pow(+0, -Inf)
// pow(+0, -Pi)
// pow(+0, -3)
// pow(+0, -0)
// pow(+0, +0)
// pow(+0, 3)
// pow(+0, +Pi)
// pow(+0, +Inf)
// pow(+0, NaN)
// pow(1/2, -Inf)
// pow(1/2, +Inf)
// pow(1, -Inf) IEEE 754-2008
// pow(1, +Inf) IEEE 754-2008
// pow(1, NaN) IEEE 754-2008
// pow(+Pi, -Inf)
// pow(+Pi, -0)
// pow(+Pi, +0)
// pow(+Pi, 1)
// pow(+Pi, +Inf)
// pow(+Pi, NaN)
// pow(+Inf, -Pi)
// pow(+Inf, -0)
// pow(+Inf, +0)
// pow(+Inf, 1)
// pow(+Inf, Pi)
// pow(+Inf, NaN)
// pow(NaN, -Pi)
// pow(NaN, -0)
// pow(NaN, +0)
// pow(NaN, 1)
// pow(NaN, +Pi)
// pow(NaN, NaN)
// Issue #7394 overflow checks
// pow(2, float64(1 << 32))
// pow(2, -float64(1 << 32))
// pow(-2, float64(1<<32 + 1))
// pow(1/2, float64(1 << 45))
// pow(1/2, -float64(1 << 45))
// pow(Nextafter(1, 2), float64(1 << 63))
// pow(Nextafter(1, -2), float64(1 << 63))
// pow(Nextafter(-1, 2), float64(1 << 63))
// pow(Nextafter(-1, -2), float64(1 << 63))
// Issue #57465
// pow(-0, 1e19)
// pow(-0, -1e19)
// pow(-0, 1<<53 -1)
// pow(-0, -(1<<53 -1))
internal static slice<float64> powSC = new float64[]{
    0D,
    Copysign(0D, -1D),
    1D,
    1D,
    Inf(-1),
    Inf(-1),
    Inf(1),
    Inf(1),
    NaN(),
    0D,
    NaN(),
    1D,
    1D,
    -Pi,
    NaN(),
    Inf(1),
    NaN(),
    1D,
    1D,
    NaN(),
    Inf(1),
    0D,
    Inf(1),
    Inf(1),
    Inf(1),
    Inf(-1),
    Copysign(0D, -1D),
    0D,
    0D,
    0D,
    Inf(1),
    Inf(1),
    Inf(1),
    1D,
    1D,
    0D,
    0D,
    0D,
    NaN(),
    Inf(1),
    0D,
    1D,
    1D,
    1D,
    0D,
    1D,
    1D,
    Pi,
    Inf(1),
    NaN(),
    0D,
    1D,
    1D,
    Inf(1),
    Inf(1),
    NaN(),
    NaN(),
    1D,
    1D,
    NaN(),
    NaN(),
    NaN(),
    Inf(1),
    0D,
    Inf(-1),
    0D,
    Inf(1),
    Inf(1),
    0D,
    0D,
    Inf(1),
    0D,
    Inf(1),
    Copysign(0D, -1D),
    Inf(-1)
}.slice();

internal static slice<nint> vfpow10SC = new nint[]{
    MinInt32,
    -324,
    -323,
    -50,
    -22,
    -1,
    0,
    1,
    22,
    50,
    100,
    200,
    308,
    309,
    MaxInt32
}.slice();

// pow10(MinInt32)
// pow10(-324)
// pow10(-323)
// pow10(-50)
// pow10(-22)
// pow10(-1)
// pow10(0)
// pow10(1)
// pow10(22)
// pow10(50)
// pow10(100)
// pow10(200)
// pow10(308)
// pow10(309)
// pow10(MaxInt32)
internal static slice<float64> pow10SC = new float64[]{
    0D,
    0D,
    1.0e-323D,
    1.0e-50D,
    1.0e-22D,
    1.0e-1D,
    1.0e0D,
    1.0e1D,
    1.0e22D,
    1.0e50D,
    1.0e100D,
    1.0e200D,
    1.0e308D,
    Inf(1),
    Inf(1)
}.slice();

// denormal
// 0.5-epsilon
// 0.5+epsilon
// 1 bit fraction
// 1 bit fraction, rounding to 0 bit fraction
// large integer
internal static slice<array<float64>> vfroundSC = GoReflect.WithElemDims(new array<float64>[]{
    new float64[]{0D, 0D}.array(),
    new float64[]{1.390671161567e-309D, 0D}.array(),
    new float64[]{0.49999999999999994D, 0D}.array(),
    new float64[]{0.5D, 1D}.array(),
    new float64[]{0.5000000000000001D, 1D}.array(),
    new float64[]{-1.5D, -2D}.array(),
    new float64[]{-2.5D, -3D}.array(),
    new float64[]{NaN(), NaN()}.array(),
    new float64[]{Inf(1), Inf(1)}.array(),
    new float64[]{2251799813685249.5D, 2251799813685250D}.array(),
    new float64[]{2251799813685250.5D, 2251799813685251D}.array(),
    new float64[]{4503599627370495.5D, 4503599627370496D}.array(),
    new float64[]{4503599627370497D, 4503599627370497D}.array()
}.slice(), 2);

// denormal
// 0.5-epsilon
// 0.5+epsilon
// 1 bit fraction
// 1 bit fraction, rounding to 0 bit fraction
// large integer
internal static slice<array<float64>> vfroundEvenSC = GoReflect.WithElemDims(new array<float64>[]{
    new float64[]{0D, 0D}.array(),
    new float64[]{1.390671161567e-309D, 0D}.array(),
    new float64[]{0.49999999999999994D, 0D}.array(),
    new float64[]{0.5D, 0D}.array(),
    new float64[]{0.5000000000000001D, 1D}.array(),
    new float64[]{-1.5D, -2D}.array(),
    new float64[]{-2.5D, -2D}.array(),
    new float64[]{NaN(), NaN()}.array(),
    new float64[]{Inf(1), Inf(1)}.array(),
    new float64[]{2251799813685249.5D, 2251799813685250D}.array(),
    new float64[]{2251799813685250.5D, 2251799813685250D}.array(),
    new float64[]{4503599627370495.5D, 4503599627370496D}.array(),
    new float64[]{4503599627370497D, 4503599627370497D}.array()
}.slice(), 2);

internal static slice<float64> vfsignbitSC = new float64[]{
    Inf(-1),
    Copysign(0D, -1D),
    0D,
    Inf(1),
    NaN()
}.slice();

internal static slice<bool> signbitSC = new bool[]{
    true,
    true,
    false,
    false,
    false
}.slice();

internal static slice<float64> vfsinSC = new float64[]{
    Inf(-1),
    Copysign(0D, -1D),
    0D,
    Inf(1),
    NaN()
}.slice();

internal static slice<float64> sinSC = new float64[]{
    NaN(),
    Copysign(0D, -1D),
    0D,
    NaN(),
    NaN()
}.slice();

internal static slice<float64> vfsinhSC = new float64[]{
    Inf(-1),
    Copysign(0D, -1D),
    0D,
    Inf(1),
    NaN()
}.slice();

internal static slice<float64> sinhSC = new float64[]{
    Inf(-1),
    Copysign(0D, -1D),
    0D,
    Inf(1),
    NaN()
}.slice();

// subnormal; see https://golang.org/issue/13013
internal static slice<float64> vfsqrtSC = new float64[]{
    Inf(-1),
    -Pi,
    Copysign(0D, -1D),
    0D,
    Inf(1),
    NaN(),
    Float64frombits(2)
}.slice();

internal static slice<float64> sqrtSC = new float64[]{
    NaN(),
    NaN(),
    Copysign(0D, -1D),
    0D,
    Inf(1),
    NaN(),
    3.1434555694052576e-162D
}.slice();

internal static slice<float64> vftanhSC = new float64[]{
    Inf(-1),
    Copysign(0D, -1D),
    0D,
    Inf(1),
    NaN()
}.slice();

internal static slice<float64> tanhSC = new float64[]{
    -1D,
    Copysign(0D, -1D),
    0D,
    1D,
    NaN()
}.slice();

internal static slice<float64> vfy0SC = new float64[]{
    Inf(-1),
    0D,
    Inf(1),
    NaN(),
    -1D
}.slice();

internal static slice<float64> y0SC = new float64[]{
    NaN(),
    Inf(-1),
    0D,
    NaN(),
    NaN()
}.slice();

internal static slice<float64> y1SC = new float64[]{
    NaN(),
    Inf(-1),
    0D,
    NaN(),
    NaN()
}.slice();

internal static slice<float64> y2SC = new float64[]{
    NaN(),
    Inf(-1),
    0D,
    NaN(),
    NaN()
}.slice();

internal static slice<float64> yM3SC = new float64[]{
    NaN(),
    Inf(1),
    0D,
    NaN(),
    NaN()
}.slice();

// arguments and expected results for boundary cases
public static UntypedFloat SmallestNormalFloat64 => 2.2250738585072014e-308; // 2**-1022

public static UntypedFloat LargestSubnormalFloat64 => /* SmallestNormalFloat64 - SmallestNonzeroFloat64 */ 2.225073858507201e-308;

internal static slice<float64> vffrexpBC = new float64[]{
    SmallestNormalFloat64,
    LargestSubnormalFloat64,
    SmallestNonzeroFloat64,
    MaxFloat64,
    -SmallestNormalFloat64,
    -LargestSubnormalFloat64,
    -SmallestNonzeroFloat64,
    -MaxFloat64
}.slice();

internal static slice<fi> frexpBC = new fi[]{
    new(0.5D, -1021),
    new(0.99999999999999978D, -1022),
    new(0.5D, -1073),
    new(0.99999999999999989D, 1024),
    new(-0.5D, -1021),
    new(-0.99999999999999978D, -1022),
    new(-0.5D, -1073),
    new(-0.99999999999999989D, 1024)
}.slice();

internal static slice<fi> vfldexpBC = new fi[]{
    new(SmallestNormalFloat64, -52),
    new(LargestSubnormalFloat64, -51),
    new(SmallestNonzeroFloat64, 1074),
    new(MaxFloat64, -(1023 + 1074)),
    new(1D, -1075),
    new(-1D, -1075),
    new(1D, 1024),
    new(-1D, 1024),
    new(1.0000000000000002D, -1075),
    new(1D, -1075)
}.slice();

// 2**-1073
// 2**-1073
internal static slice<float64> ldexpBC = new float64[]{
    SmallestNonzeroFloat64,
    1e-323D,
    1D,
    1e-323D,
    0D,
    Copysign(0D, -1D),
    Inf(1),
    Inf(-1),
    SmallestNonzeroFloat64,
    0D
}.slice();

internal static slice<float64> logbBC = new float64[]{
    -1022D,
    -1023D,
    -1074D,
    1023D,
    -1022D,
    -1023D,
    -1074D,
    1023D
}.slice();

// Large exponent spread
// Effective addition
// Effective subtraction
// Overflow
// Finite x and y, but non-finite z.
// Special
// Random
// Issue #61130
// Test cases were generated with Berkeley TestFloat-3e/testfloat_gen.
// http://www.jhauser.us/arithmetic/TestFloat.html.
// The default rounding mode is selected (nearest/even), and exception flags are ignored.

partial struct fmaCᴛ1 /*dyn*/ {
    internal float64 x, y, z, want;
}
internal static slice<fmaCᴛ1> fmaC = new fmaCᴛ1[]{
    new(-3.999999999999087D, -1.1123914289620494e-16D, -7.999877929687506D, -7.999877929687505D),
    new(-262112.0000004768D, -0.06251525855623184D, 1.1102230248837136e-16D, 16385.99945072085D),
    new(-6.462348523533467e-27D, -2.3763644720331857e-211D, 4.000000000931324D, 4.000000000931324D),
    new(-2.0000000037252907D, 6.7904383376e-313D, -3.3951933161e-313D, -1.697607001654e-312D),
    new(-0.12499999999999999D, 512.007568359375D, -1.4193627164960366e-16D, -64.00094604492188D),
    new(-2.7550648847397148e-39D, -3.4028301595800694e+38D, 0.9960937495343386D, 1.9335955376735676D),
    new(5.723369164769208e+24D, 3.8149300927159385e-06D, 1.84489958778182e+19D, 4.028324913621874e+19D),
    new(-0.4843749999990904D, -3.6893487872543293e+19D, 9.223653786709391e+18D, 2.7093936974938993e+19D),
    new(-3.8146972665201165e-06D, 4.2949672959999385e+09D, -2.2204460489938386e-16D, -16384.000003844263D),
    new(6.98156394130982e-309D, -1.1072962560000002e+09D, -4.4414561548793455e-308D, -7.73065965765153e-300D),
    new(5e-324D, 4.5D, -2e-323D, 0D),
    new(5e-324D, 7D, -3.5e-323D, 0D),
    new(5e-324D, 0.5000000000000001D, -5e-324D, Copysign(0D, -1D)),
    new(-2.1240680525e-314D, -1.233647078189316e+308D, -0.25781249999954525D, -0.25780987964919844D),
    new(8.579992955364441e-308D, 0.6037391876780558D, -4.4501307410480706e-308D, 7.29947236107098e-309D),
    new(-4.450143471986689e-308D, -0.9960937499927239D, -4.450419332475649e-308D, -1.7659233458788e-310D),
    new(1.4932076393918112D, -2.2248022430460833e-308D, 4.449875571054211e-308D, 1.127783865601762e-308D),
    new(-2.288020632214759e+38D, -8.98846570988901e+307D, 1.7696041796300924e+308D, Inf(0)),
    new(1.4888652783208255e+308D, -9.007199254742012e+15D, -6.807282911929205e+38D, Inf(-1)),
    new(9.142703268902826e+192D, -1.3504889569802838e+296D, -1.9082200803806996e-89D, Inf(-1)),
    new(31.99218749627471D, -1.7976930544991702e+308D, Inf(0), Inf(0)),
    new(-1.7976931281784667e+308D, -2.0009765625002265D, Inf(-1), Inf(-1)),
    new(0D, 0D, 0D, 0D),
    new(Copysign(0D, -1D), 0D, 0D, 0D),
    new(0D, 0D, Copysign(0D, -1D), 0D),
    new(Copysign(0D, -1D), 0D, Copysign(0D, -1D), Copysign(0D, -1D)),
    new(-1.1754226043408471e-38D, NaN(), Inf(0), NaN()),
    new(0D, 0D, 2.22507385643494e-308D, 2.22507385643494e-308D),
    new(-8.65697792e+09D, NaN(), -7.516192799999999e+09D, NaN()),
    new(-0.00012207403779029757D, 3.221225471996093e+09D, NaN(), NaN()),
    new(Inf(-1), 0.1252441407414153D, -1.387184532981584e-76D, Inf(-1)),
    new(Inf(0), 1.525878907671432e-05D, -9.214364835452549e+18D, Inf(0)),
    new(0.1777916152213626D, -32.000015266239636D, -2.2204459148334633e-16D, -5.689334401293007D),
    new(-2.0816681711722314e-16D, -0.4997558592585846D, -0.9465627129124969D, -0.9465627129124968D),
    new(-1.9999997615814211D, 1.8518819259933516e+19D, 16.874999999999996D, -3.703763410463646e+19D),
    new(-0.12499994039717421D, 32767.99999976135D, -2.0752587082923246e+19D, -2.075258708292325e+19D),
    new(7.705600568510257e-34D, -1.801432979000528e+16D, -0.17224197722973714D, -0.17224197722973716D),
    new(3.8988133103758913e-308D, -0.9848632812499999D, 3.893879244098556e-308D, 5.40811742605814e-310D),
    new(-0.012651981190687427D, 6.911985574912436e+38D, 6.669240527007144e+18D, -8.745031148409496e+36D),
    new(4.612811918325842e+18D, 1.4901161193847641e-08D, 2.6077032311277997e-08D, 6.873625395187494e+10D),
    new(-9.094947033611148e-13D, 4.450691014249257e-308D, 2.086006742350485e-308D, 2.086006742346437e-308D),
    new(-7.751454006381804e-05D, 5.588653777189071e-308D, -2.2207280111272877e-308D, -2.2211612130544025e-308D),
    new(-1D, 1D, 1D, 0D),
    new(1D, 1D, -1D, 0D)
}.slice();

internal static slice<float32> sqrt32 = new float32[]{
    0F,
    (float32)Copysign(0D, -1D),
    (float32)NaN(),
    (float32)Inf(1),
    (float32)Inf(-1),
    1F,
    2F,
    -2F,
    4.9790119248836735e+00F,
    7.7388724745781045e+00F,
    -2.7688005719200159e-01F,
    -5.0106036182710749e+00F
}.slice();

internal static bool tolerance(float64 a, float64 b, float64 e) {
    // Multiplying by e here can underflow denormal values to zero.
    // Check a==b so that at least if a and b are small and identical
    // we say they match.
    if (a == b) {
        return true;
    }
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

internal static bool close(float64 a, float64 b) {
    return tolerance(a, b, 1e-14D);
}

internal static bool veryclose(float64 a, float64 b) {
    return tolerance(a, b, 4e-16D);
}

internal static bool soclose(float64 a, float64 b, float64 e) {
    return tolerance(a, b, e);
}

internal static bool alike(float64 a, float64 b) {
    switch (ᐧ) {
    case {} when IsNaN(a) && IsNaN(b): {
        return true;
    }
    case {} when a == b: {
        return Signbit(a) == Signbit(b);
    }}

    return false;
}

public static void TestNaN(ж<testing.T> Ꮡt) {
    var f64 = NaN();
    if (f64 == f64) {
        Ꮡt.Fatalf("NaN() returns %g, expected NaN"u8, f64);
    }
    var f32 = (float32)f64;
    if (f32 == f32) {
        Ꮡt.Fatalf("float32(NaN()) is %g, expected NaN"u8, f32);
    }
}

public static void TestAcos(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        var a = vf[i] / 10D;
        {
            var f = Acos(a); if (!close(acos[i], f)) {
                Ꮡt.Errorf("Acos(%g) = %g, want %g"u8, a, f, acos[i]);
            }
        }
    }
    for (nint i = 0; i < len(vfacosSC); i++) {
        {
            var f = Acos(vfacosSC[i]); if (!alike(acosSC[i], f)) {
                Ꮡt.Errorf("Acos(%g) = %g, want %g"u8, vfacosSC[i], f, acosSC[i]);
            }
        }
    }
}

public static void TestAcosh(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        var a = 1D + Abs(vf[i]);
        {
            var f = Acosh(a); if (!veryclose(acosh[i], f)) {
                Ꮡt.Errorf("Acosh(%g) = %g, want %g"u8, a, f, acosh[i]);
            }
        }
    }
    for (nint i = 0; i < len(vfacoshSC); i++) {
        {
            var f = Acosh(vfacoshSC[i]); if (!alike(acoshSC[i], f)) {
                Ꮡt.Errorf("Acosh(%g) = %g, want %g"u8, vfacoshSC[i], f, acoshSC[i]);
            }
        }
    }
}

public static void TestAsin(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        var a = vf[i] / 10D;
        {
            var f = Asin(a); if (!veryclose(asin[i], f)) {
                Ꮡt.Errorf("Asin(%g) = %g, want %g"u8, a, f, asin[i]);
            }
        }
    }
    for (nint i = 0; i < len(vfasinSC); i++) {
        {
            var f = Asin(vfasinSC[i]); if (!alike(asinSC[i], f)) {
                Ꮡt.Errorf("Asin(%g) = %g, want %g"u8, vfasinSC[i], f, asinSC[i]);
            }
        }
    }
}

public static void TestAsinh(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        {
            var f = Asinh(vf[i]); if (!veryclose(asinh[i], f)) {
                Ꮡt.Errorf("Asinh(%g) = %g, want %g"u8, vf[i], f, asinh[i]);
            }
        }
    }
    for (nint i = 0; i < len(vfasinhSC); i++) {
        {
            var f = Asinh(vfasinhSC[i]); if (!alike(asinhSC[i], f)) {
                Ꮡt.Errorf("Asinh(%g) = %g, want %g"u8, vfasinhSC[i], f, asinhSC[i]);
            }
        }
    }
}

public static void TestAtan(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        {
            var f = Atan(vf[i]); if (!veryclose(atan[i], f)) {
                Ꮡt.Errorf("Atan(%g) = %g, want %g"u8, vf[i], f, atan[i]);
            }
        }
    }
    for (nint i = 0; i < len(vfatanSC); i++) {
        {
            var f = Atan(vfatanSC[i]); if (!alike(atanSC[i], f)) {
                Ꮡt.Errorf("Atan(%g) = %g, want %g"u8, vfatanSC[i], f, atanSC[i]);
            }
        }
    }
}

public static void TestAtanh(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        var a = vf[i] / 10D;
        {
            var f = Atanh(a); if (!veryclose(atanh[i], f)) {
                Ꮡt.Errorf("Atanh(%g) = %g, want %g"u8, a, f, atanh[i]);
            }
        }
    }
    for (nint i = 0; i < len(vfatanhSC); i++) {
        {
            var f = Atanh(vfatanhSC[i]); if (!alike(atanhSC[i], f)) {
                Ꮡt.Errorf("Atanh(%g) = %g, want %g"u8, vfatanhSC[i], f, atanhSC[i]);
            }
        }
    }
}

public static void TestAtan2(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        {
            var f = Atan2(10D, vf[i]); if (!veryclose(atan2[i], f)) {
                Ꮡt.Errorf("Atan2(10, %g) = %g, want %g"u8, vf[i], f, atan2[i]);
            }
        }
    }
    for (nint i = 0; i < len(vfatan2SC); i++) {
        {
            var f = Atan2(vfatan2SC[i][0], vfatan2SC[i][1]); if (!alike(atan2SC[i], f)) {
                Ꮡt.Errorf("Atan2(%g, %g) = %g, want %g"u8, vfatan2SC[i][0], vfatan2SC[i][1], f, atan2SC[i]);
            }
        }
    }
}

public static void TestCbrt(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        {
            var f = Cbrt(vf[i]); if (!veryclose(cbrt[i], f)) {
                Ꮡt.Errorf("Cbrt(%g) = %g, want %g"u8, vf[i], f, cbrt[i]);
            }
        }
    }
    for (nint i = 0; i < len(vfcbrtSC); i++) {
        {
            var f = Cbrt(vfcbrtSC[i]); if (!alike(cbrtSC[i], f)) {
                Ꮡt.Errorf("Cbrt(%g) = %g, want %g"u8, vfcbrtSC[i], f, cbrtSC[i]);
            }
        }
    }
}

public static void TestCeil(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        {
            var f = Ceil(vf[i]); if (!alike(ceil[i], f)) {
                Ꮡt.Errorf("Ceil(%g) = %g, want %g"u8, vf[i], f, ceil[i]);
            }
        }
    }
    for (nint i = 0; i < len(vfceilSC); i++) {
        {
            var f = Ceil(vfceilSC[i]); if (!alike(ceilSC[i], f)) {
                Ꮡt.Errorf("Ceil(%g) = %g, want %g"u8, vfceilSC[i], f, ceilSC[i]);
            }
        }
    }
}

public static void TestCopysign(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        {
            var f = Copysign(vf[i], -1D); if (copysign[i] != f) {
                Ꮡt.Errorf("Copysign(%g, -1) = %g, want %g"u8, vf[i], f, copysign[i]);
            }
        }
    }
    for (nint i = 0; i < len(vf); i++) {
        {
            var f = Copysign(vf[i], 1D); if (-copysign[i] != f) {
                Ꮡt.Errorf("Copysign(%g, 1) = %g, want %g"u8, vf[i], f, -copysign[i]);
            }
        }
    }
    for (nint i = 0; i < len(vfcopysignSC); i++) {
        {
            var f = Copysign(vfcopysignSC[i], -1D); if (!alike(copysignSC[i], f)) {
                Ꮡt.Errorf("Copysign(%g, -1) = %g, want %g"u8, vfcopysignSC[i], f, copysignSC[i]);
            }
        }
    }
}

public static void TestCos(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        {
            var f = Cos(vf[i]); if (!veryclose(cos[i], f)) {
                Ꮡt.Errorf("Cos(%g) = %g, want %g"u8, vf[i], f, cos[i]);
            }
        }
    }
    for (nint i = 0; i < len(vfcosSC); i++) {
        {
            var f = Cos(vfcosSC[i]); if (!alike(cosSC[i], f)) {
                Ꮡt.Errorf("Cos(%g) = %g, want %g"u8, vfcosSC[i], f, cosSC[i]);
            }
        }
    }
}

public static void TestCosh(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        {
            var f = Cosh(vf[i]); if (!close(cosh[i], f)) {
                Ꮡt.Errorf("Cosh(%g) = %g, want %g"u8, vf[i], f, cosh[i]);
            }
        }
    }
    for (nint i = 0; i < len(vfcoshSC); i++) {
        {
            var f = Cosh(vfcoshSC[i]); if (!alike(coshSC[i], f)) {
                Ꮡt.Errorf("Cosh(%g) = %g, want %g"u8, vfcoshSC[i], f, coshSC[i]);
            }
        }
    }
}

public static void TestErf(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        var a = vf[i] / 10D;
        {
            var f = Erf(a); if (!veryclose(erf[i], f)) {
                Ꮡt.Errorf("Erf(%g) = %g, want %g"u8, a, f, erf[i]);
            }
        }
    }
    for (nint i = 0; i < len(vferfSC); i++) {
        {
            var f = Erf(vferfSC[i]); if (!alike(erfSC[i], f)) {
                Ꮡt.Errorf("Erf(%g) = %g, want %g"u8, vferfSC[i], f, erfSC[i]);
            }
        }
    }
}

public static void TestErfc(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        var a = vf[i] / 10D;
        {
            var f = Erfc(a); if (!veryclose(erfc[i], f)) {
                Ꮡt.Errorf("Erfc(%g) = %g, want %g"u8, a, f, erfc[i]);
            }
        }
    }
    for (nint i = 0; i < len(vferfcSC); i++) {
        {
            var f = Erfc(vferfcSC[i]); if (!alike(erfcSC[i], f)) {
                Ꮡt.Errorf("Erfc(%g) = %g, want %g"u8, vferfcSC[i], f, erfcSC[i]);
            }
        }
    }
}

public static void TestErfinv(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        var a = vf[i] / 10D;
        {
            var f = Erfinv(a); if (!veryclose(erfinv[i], f)) {
                Ꮡt.Errorf("Erfinv(%g) = %g, want %g"u8, a, f, erfinv[i]);
            }
        }
    }
    for (nint i = 0; i < len(vferfinvSC); i++) {
        {
            var f = Erfinv(vferfinvSC[i]); if (!alike(erfinvSC[i], f)) {
                Ꮡt.Errorf("Erfinv(%g) = %g, want %g"u8, vferfinvSC[i], f, erfinvSC[i]);
            }
        }
    }
    for (var x = -0.9D; x <= 0.90D; x += 1e-2D) {
        {
            var f = Erf(Erfinv(x)); if (!close(x, f)) {
                Ꮡt.Errorf("Erf(Erfinv(%g)) = %g, want %g"u8, x, f, x);
            }
        }
    }
    for (var x = -0.9D; x <= 0.90D; x += 1e-2D) {
        {
            var f = Erfinv(Erf(x)); if (!close(x, f)) {
                Ꮡt.Errorf("Erfinv(Erf(%g)) = %g, want %g"u8, x, f, x);
            }
        }
    }
}

public static void TestErfcinv(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        var a = 1.0D - (vf[i] / 10D);
        {
            var f = Erfcinv(a); if (!veryclose(erfinv[i], f)) {
                Ꮡt.Errorf("Erfcinv(%g) = %g, want %g"u8, a, f, erfinv[i]);
            }
        }
    }
    for (nint i = 0; i < len(vferfcinvSC); i++) {
        {
            var f = Erfcinv(vferfcinvSC[i]); if (!alike(erfcinvSC[i], f)) {
                Ꮡt.Errorf("Erfcinv(%g) = %g, want %g"u8, vferfcinvSC[i], f, erfcinvSC[i]);
            }
        }
    }
    for (var x = 0.1D; x <= 1.9D; x += 1e-2D) {
        {
            var f = Erfc(Erfcinv(x)); if (!close(x, f)) {
                Ꮡt.Errorf("Erfc(Erfcinv(%g)) = %g, want %g"u8, x, f, x);
            }
        }
    }
    for (var x = 0.1D; x <= 1.9D; x += 1e-2D) {
        {
            var f = Erfcinv(Erfc(x)); if (!close(x, f)) {
                Ꮡt.Errorf("Erfcinv(Erfc(%g)) = %g, want %g"u8, x, f, x);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string expˢ = "Exp"u8;
internal static readonly @string expGoˢ = "ExpGo"u8;

public static void TestExp(ж<testing.T> Ꮡt) {
    testExp(Ꮡt, Exp, expˢ);
    testExp(Ꮡt, math_internal_test_package.ExpGo, expGoˢ);
}

internal static void testExp(ж<testing.T> Ꮡt, Func<float64, float64> Exp, @string name) {
    for (nint i = 0; i < len(vf); i++) {
        {
            var f = Exp(vf[i]); if (!veryclose(exp[i], f)) {
                Ꮡt.Errorf("%s(%g) = %g, want %g"u8, name, vf[i], f, exp[i]);
            }
        }
    }
    for (nint i = 0; i < len(vfexpSC); i++) {
        {
            var f = Exp(vfexpSC[i]); if (!alike(expSC[i], f)) {
                Ꮡt.Errorf("%s(%g) = %g, want %g"u8, name, vfexpSC[i], f, expSC[i]);
            }
        }
    }
}

public static void TestExpm1(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        var a = vf[i] / 100D;
        {
            var f = Expm1(a); if (!veryclose(expm1[i], f)) {
                Ꮡt.Errorf("Expm1(%g) = %g, want %g"u8, a, f, expm1[i]);
            }
        }
    }
    for (nint i = 0; i < len(vf); i++) {
        var a = vf[i] * 10D;
        {
            var f = Expm1(a); if (!close(expm1Large[i], f)) {
                Ꮡt.Errorf("Expm1(%g) = %g, want %g"u8, a, f, expm1Large[i]);
            }
        }
    }
    for (nint i = 0; i < len(vfexpm1SC); i++) {
        {
            var f = Expm1(vfexpm1SC[i]); if (!alike(expm1SC[i], f)) {
                Ꮡt.Errorf("Expm1(%g) = %g, want %g"u8, vfexpm1SC[i], f, expm1SC[i]);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string exp2ˢ = "Exp2"u8;
internal static readonly @string exp2Goˢ = "Exp2Go"u8;

public static void TestExp2(ж<testing.T> Ꮡt) {
    testExp2(Ꮡt, Exp2, exp2ˢ);
    testExp2(Ꮡt, math_internal_test_package.Exp2Go, exp2Goˢ);
}

internal static void testExp2(ж<testing.T> Ꮡt, Func<float64, float64> Exp2, @string name) {
    for (nint i = 0; i < len(vf); i++) {
        {
            var f = Exp2(vf[i]); if (!close(exp2[i], f)) {
                Ꮡt.Errorf("%s(%g) = %g, want %g"u8, name, vf[i], f, exp2[i]);
            }
        }
    }
    for (nint i = 0; i < len(vfexp2SC); i++) {
        {
            var f = Exp2(vfexp2SC[i]); if (!alike(exp2SC[i], f)) {
                Ꮡt.Errorf("%s(%g) = %g, want %g"u8, name, vfexp2SC[i], f, exp2SC[i]);
            }
        }
    }
    for (nint n = -1074; n < 1024; n++) {
        var f = Exp2((float64)n);
        var vfΔ1 = Ldexp(1D, n);
        if (f != vfΔ1) {
            Ꮡt.Errorf("%s(%d) = %g, want %g"u8, name, n, f, vfΔ1);
        }
    }
}

public static void TestAbs(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        {
            var f = Abs(vf[i]); if (fabs[i] != f) {
                Ꮡt.Errorf("Abs(%g) = %g, want %g"u8, vf[i], f, fabs[i]);
            }
        }
    }
    for (nint i = 0; i < len(vffabsSC); i++) {
        {
            var f = Abs(vffabsSC[i]); if (!alike(fabsSC[i], f)) {
                Ꮡt.Errorf("Abs(%g) = %g, want %g"u8, vffabsSC[i], f, fabsSC[i]);
            }
        }
    }
}

public static void TestDim(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        {
            var f = Dim(vf[i], 0D); if (fdim[i] != f) {
                Ꮡt.Errorf("Dim(%g, %g) = %g, want %g"u8, vf[i], 0.0D, f, fdim[i]);
            }
        }
    }
    for (nint i = 0; i < len(vffdimSC); i++) {
        {
            var f = Dim(vffdimSC[i][0], vffdimSC[i][1]); if (!alike(fdimSC[i], f)) {
                Ꮡt.Errorf("Dim(%g, %g) = %g, want %g"u8, vffdimSC[i][0], vffdimSC[i][1], f, fdimSC[i]);
            }
        }
    }
    for (nint i = 0; i < len(vffdim2SC); i++) {
        {
            var f = Dim(vffdim2SC[i][0], vffdim2SC[i][1]); if (!alike(fdimSC[i], f)) {
                Ꮡt.Errorf("Dim(%g, %g) = %g, want %g"u8, vffdim2SC[i][0], vffdim2SC[i][1], f, fdimSC[i]);
            }
        }
    }
}

public static void TestFloor(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        {
            var f = Floor(vf[i]); if (!alike(floor[i], f)) {
                Ꮡt.Errorf("Floor(%g) = %g, want %g"u8, vf[i], f, floor[i]);
            }
        }
    }
    for (nint i = 0; i < len(vfceilSC); i++) {
        {
            var f = Floor(vfceilSC[i]); if (!alike(floorSC[i], f)) {
                Ꮡt.Errorf("Floor(%g) = %g, want %g"u8, vfceilSC[i], f, floorSC[i]);
            }
        }
    }
}

public static void TestMax(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        {
            var f = Max(vf[i], ceil[i]); if (ceil[i] != f) {
                Ꮡt.Errorf("Max(%g, %g) = %g, want %g"u8, vf[i], ceil[i], f, ceil[i]);
            }
        }
    }
    for (nint i = 0; i < len(vffdimSC); i++) {
        {
            var f = Max(vffdimSC[i][0], vffdimSC[i][1]); if (!alike(fmaxSC[i], f)) {
                Ꮡt.Errorf("Max(%g, %g) = %g, want %g"u8, vffdimSC[i][0], vffdimSC[i][1], f, fmaxSC[i]);
            }
        }
    }
    for (nint i = 0; i < len(vffdim2SC); i++) {
        {
            var f = Max(vffdim2SC[i][0], vffdim2SC[i][1]); if (!alike(fmaxSC[i], f)) {
                Ꮡt.Errorf("Max(%g, %g) = %g, want %g"u8, vffdim2SC[i][0], vffdim2SC[i][1], f, fmaxSC[i]);
            }
        }
    }
}

public static void TestMin(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        {
            var f = Min(vf[i], floor[i]); if (floor[i] != f) {
                Ꮡt.Errorf("Min(%g, %g) = %g, want %g"u8, vf[i], floor[i], f, floor[i]);
            }
        }
    }
    for (nint i = 0; i < len(vffdimSC); i++) {
        {
            var f = Min(vffdimSC[i][0], vffdimSC[i][1]); if (!alike(fminSC[i], f)) {
                Ꮡt.Errorf("Min(%g, %g) = %g, want %g"u8, vffdimSC[i][0], vffdimSC[i][1], f, fminSC[i]);
            }
        }
    }
    for (nint i = 0; i < len(vffdim2SC); i++) {
        {
            var f = Min(vffdim2SC[i][0], vffdim2SC[i][1]); if (!alike(fminSC[i], f)) {
                Ꮡt.Errorf("Min(%g, %g) = %g, want %g"u8, vffdim2SC[i][0], vffdim2SC[i][1], f, fminSC[i]);
            }
        }
    }
}

public static void TestMod(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        {
            var f = Mod(10D, vf[i]); if (fmod[i] != f) {
                Ꮡt.Errorf("Mod(10, %g) = %g, want %g"u8, vf[i], f, fmod[i]);
            }
        }
    }
    for (nint i = 0; i < len(vffmodSC); i++) {
        {
            var f = Mod(vffmodSC[i][0], vffmodSC[i][1]); if (!alike(fmodSC[i], f)) {
                Ꮡt.Errorf("Mod(%g, %g) = %g, want %g"u8, vffmodSC[i][0], vffmodSC[i][1], f, fmodSC[i]);
            }
        }
    }
    // verify precision of result for extreme inputs
    {
        var f = Mod(5.9790119248836734e+200D, 1.1258465975523544D); if (0.6447968302508578D != f) {
            Ꮡt.Errorf("Remainder(5.9790119248836734e+200, 1.1258465975523544) = %g, want 0.6447968302508578"u8, f);
        }
    }
}

public static void TestFrexp(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        {
            var (f, j) = Frexp(vf[i]); if (!veryclose(frexp[i].f, f) || frexp[i].i != j) {
                Ꮡt.Errorf("Frexp(%g) = %g, %d, want %g, %d"u8, vf[i], f, j, frexp[i].f, frexp[i].i);
            }
        }
    }
    for (nint i = 0; i < len(vffrexpSC); i++) {
        {
            var (f, j) = Frexp(vffrexpSC[i]); if (!alike(frexpSC[i].f, f) || frexpSC[i].i != j) {
                Ꮡt.Errorf("Frexp(%g) = %g, %d, want %g, %d"u8, vffrexpSC[i], f, j, frexpSC[i].f, frexpSC[i].i);
            }
        }
    }
    for (nint i = 0; i < len(vffrexpBC); i++) {
        {
            var (f, j) = Frexp(vffrexpBC[i]); if (!alike(frexpBC[i].f, f) || frexpBC[i].i != j) {
                Ꮡt.Errorf("Frexp(%g) = %g, %d, want %g, %d"u8, vffrexpBC[i], f, j, frexpBC[i].f, frexpBC[i].i);
            }
        }
    }
}

public static void TestGamma(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        {
            var f = Gamma(vf[i]); if (!close(gamma[i], f)) {
                Ꮡt.Errorf("Gamma(%g) = %g, want %g"u8, vf[i], f, gamma[i]);
            }
        }
    }
    foreach (var (_, vᴛ1) in vfgamma) {
        var g = vᴛ1.Clone();

        var f = Gamma(g[0]);
        bool ok = default!;
        if (IsNaN(g[1]) || IsInf(g[1], 0) || g[1] == 0D || f == 0D){
            ok = alike(g[1], f);
        } else 
        if (g[0] > -50D && g[0] <= 171D){
            ok = veryclose(g[1], f);
        } else {
            ok = close(g[1], f);
        }
        if (!ok) {
            Ꮡt.Errorf("Gamma(%g) = %g, want %g"u8, g[0], f, g[1]);
        }
    }
}

public static void TestHypot(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        var a = Abs(1e200D * tanh[i] * Sqrt(2D));
        {
            var f = Hypot(1e200D * tanh[i], 1e200D * tanh[i]); if (!veryclose(a, f)) {
                Ꮡt.Errorf("Hypot(%g, %g) = %g, want %g"u8, 1e200D * tanh[i], 1e200D * tanh[i], f, a);
            }
        }
    }
    for (nint i = 0; i < len(vfhypotSC); i++) {
        {
            var f = Hypot(vfhypotSC[i][0], vfhypotSC[i][1]); if (!alike(hypotSC[i], f)) {
                Ꮡt.Errorf("Hypot(%g, %g) = %g, want %g"u8, vfhypotSC[i][0], vfhypotSC[i][1], f, hypotSC[i]);
            }
        }
    }
}

public static void TestHypotGo(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        var a = Abs(1e200D * tanh[i] * Sqrt(2D));
        {
            var f = math_internal_test_package.HypotGo(1e200D * tanh[i], 1e200D * tanh[i]); if (!veryclose(a, f)) {
                Ꮡt.Errorf("HypotGo(%g, %g) = %g, want %g"u8, 1e200D * tanh[i], 1e200D * tanh[i], f, a);
            }
        }
    }
    for (nint i = 0; i < len(vfhypotSC); i++) {
        {
            var f = math_internal_test_package.HypotGo(vfhypotSC[i][0], vfhypotSC[i][1]); if (!alike(hypotSC[i], f)) {
                Ꮡt.Errorf("HypotGo(%g, %g) = %g, want %g"u8, vfhypotSC[i][0], vfhypotSC[i][1], f, hypotSC[i]);
            }
        }
    }
}

public static void TestIlogb(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        nint a = frexp[i].i - 1; // adjust because fr in the interval [½, 1)
        {
            nint e = Ilogb(vf[i]); if (a != e) {
                Ꮡt.Errorf("Ilogb(%g) = %d, want %d"u8, vf[i], e, a);
            }
        }
    }
    for (nint i = 0; i < len(vflogbSC); i++) {
        {
            nint e = Ilogb(vflogbSC[i]); if (ilogbSC[i] != e) {
                Ꮡt.Errorf("Ilogb(%g) = %d, want %d"u8, vflogbSC[i], e, ilogbSC[i]);
            }
        }
    }
    for (nint i = 0; i < len(vffrexpBC); i++) {
        {
            nint e = Ilogb(vffrexpBC[i]); if ((nint)logbBC[i] != e) {
                Ꮡt.Errorf("Ilogb(%g) = %d, want %d"u8, vffrexpBC[i], e, (nint)logbBC[i]);
            }
        }
    }
}

public static void TestJ0(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        {
            var f = J0(vf[i]); if (!soclose(j0[i], f, 4e-14D)) {
                Ꮡt.Errorf("J0(%g) = %g, want %g"u8, vf[i], f, j0[i]);
            }
        }
    }
    for (nint i = 0; i < len(vfj0SC); i++) {
        {
            var f = J0(vfj0SC[i]); if (!alike(j0SC[i], f)) {
                Ꮡt.Errorf("J0(%g) = %g, want %g"u8, vfj0SC[i], f, j0SC[i]);
            }
        }
    }
}

public static void TestJ1(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        {
            var f = J1(vf[i]); if (!close(j1[i], f)) {
                Ꮡt.Errorf("J1(%g) = %g, want %g"u8, vf[i], f, j1[i]);
            }
        }
    }
    for (nint i = 0; i < len(vfj0SC); i++) {
        {
            var f = J1(vfj0SC[i]); if (!alike(j1SC[i], f)) {
                Ꮡt.Errorf("J1(%g) = %g, want %g"u8, vfj0SC[i], f, j1SC[i]);
            }
        }
    }
}

public static void TestJn(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        {
            var f = Jn(2, vf[i]); if (!close(j2[i], f)) {
                Ꮡt.Errorf("Jn(2, %g) = %g, want %g"u8, vf[i], f, j2[i]);
            }
        }
        {
            var f = Jn(-3, vf[i]); if (!close(jM3[i], f)) {
                Ꮡt.Errorf("Jn(-3, %g) = %g, want %g"u8, vf[i], f, jM3[i]);
            }
        }
    }
    for (nint i = 0; i < len(vfj0SC); i++) {
        {
            var f = Jn(2, vfj0SC[i]); if (!alike(j2SC[i], f)) {
                Ꮡt.Errorf("Jn(2, %g) = %g, want %g"u8, vfj0SC[i], f, j2SC[i]);
            }
        }
        {
            var f = Jn(-3, vfj0SC[i]); if (!alike(jM3SC[i], f)) {
                Ꮡt.Errorf("Jn(-3, %g) = %g, want %g"u8, vfj0SC[i], f, jM3SC[i]);
            }
        }
    }
}

public static void TestLdexp(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        {
            var f = Ldexp(frexp[i].f, frexp[i].i); if (!veryclose(vf[i], f)) {
                Ꮡt.Errorf("Ldexp(%g, %d) = %g, want %g"u8, frexp[i].f, frexp[i].i, f, vf[i]);
            }
        }
    }
    for (nint i = 0; i < len(vffrexpSC); i++) {
        {
            var f = Ldexp(frexpSC[i].f, frexpSC[i].i); if (!alike(vffrexpSC[i], f)) {
                Ꮡt.Errorf("Ldexp(%g, %d) = %g, want %g"u8, frexpSC[i].f, frexpSC[i].i, f, vffrexpSC[i]);
            }
        }
    }
    for (nint i = 0; i < len(vfldexpSC); i++) {
        {
            var f = Ldexp(vfldexpSC[i].f, vfldexpSC[i].i); if (!alike(ldexpSC[i], f)) {
                Ꮡt.Errorf("Ldexp(%g, %d) = %g, want %g"u8, vfldexpSC[i].f, vfldexpSC[i].i, f, ldexpSC[i]);
            }
        }
    }
    for (nint i = 0; i < len(vffrexpBC); i++) {
        {
            var f = Ldexp(frexpBC[i].f, frexpBC[i].i); if (!alike(vffrexpBC[i], f)) {
                Ꮡt.Errorf("Ldexp(%g, %d) = %g, want %g"u8, frexpBC[i].f, frexpBC[i].i, f, vffrexpBC[i]);
            }
        }
    }
    for (nint i = 0; i < len(vfldexpBC); i++) {
        {
            var f = Ldexp(vfldexpBC[i].f, vfldexpBC[i].i); if (!alike(ldexpBC[i], f)) {
                Ꮡt.Errorf("Ldexp(%g, %d) = %g, want %g"u8, vfldexpBC[i].f, vfldexpBC[i].i, f, ldexpBC[i]);
            }
        }
    }
}

public static void TestLgamma(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        {
            var (f, s) = Lgamma(vf[i]); if (!close(lgamma[i].f, f) || lgamma[i].i != s) {
                Ꮡt.Errorf("Lgamma(%g) = %g, %d, want %g, %d"u8, vf[i], f, s, lgamma[i].f, lgamma[i].i);
            }
        }
    }
    for (nint i = 0; i < len(vflgammaSC); i++) {
        {
            var (f, s) = Lgamma(vflgammaSC[i]); if (!alike(lgammaSC[i].f, f) || lgammaSC[i].i != s) {
                Ꮡt.Errorf("Lgamma(%g) = %g, %d, want %g, %d"u8, vflgammaSC[i], f, s, lgammaSC[i].f, lgammaSC[i].i);
            }
        }
    }
}

public static void TestLog(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        var a = Abs(vf[i]);
        {
            var f = Log(a); if (log[i] != f) {
                Ꮡt.Errorf("Log(%g) = %g, want %g"u8, a, f, log[i]);
            }
        }
    }
    {
        var f = Log(10D); if (f != Ln10) {
            Ꮡt.Errorf("Log(%g) = %g, want %g"u8, 10.0D, f, (float64)(Ln10));
        }
    }
    for (nint i = 0; i < len(vflogSC); i++) {
        {
            var f = Log(vflogSC[i]); if (!alike(logSC[i], f)) {
                Ꮡt.Errorf("Log(%g) = %g, want %g"u8, vflogSC[i], f, logSC[i]);
            }
        }
    }
}

public static void TestLogb(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        {
            var f = Logb(vf[i]); if (logb[i] != f) {
                Ꮡt.Errorf("Logb(%g) = %g, want %g"u8, vf[i], f, logb[i]);
            }
        }
    }
    for (nint i = 0; i < len(vflogbSC); i++) {
        {
            var f = Logb(vflogbSC[i]); if (!alike(logbSC[i], f)) {
                Ꮡt.Errorf("Logb(%g) = %g, want %g"u8, vflogbSC[i], f, logbSC[i]);
            }
        }
    }
    for (nint i = 0; i < len(vffrexpBC); i++) {
        {
            var f = Logb(vffrexpBC[i]); if (!alike(logbBC[i], f)) {
                Ꮡt.Errorf("Logb(%g) = %g, want %g"u8, vffrexpBC[i], f, logbBC[i]);
            }
        }
    }
}

public static void TestLog10(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        var a = Abs(vf[i]);
        {
            var f = Log10(a); if (!veryclose(log10[i], f)) {
                Ꮡt.Errorf("Log10(%g) = %g, want %g"u8, a, f, log10[i]);
            }
        }
    }
    {
        var f = Log10(E); if (f != Log10E) {
            Ꮡt.Errorf("Log10(%g) = %g, want %g"u8, (float64)(E), f, (float64)(Log10E));
        }
    }
    for (nint i = 0; i < len(vflogSC); i++) {
        {
            var f = Log10(vflogSC[i]); if (!alike(logSC[i], f)) {
                Ꮡt.Errorf("Log10(%g) = %g, want %g"u8, vflogSC[i], f, logSC[i]);
            }
        }
    }
}

public static void TestLog1p(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        var aΔ1 = vf[i] / 100D;
        {
            var f = Log1p(aΔ1); if (!veryclose(log1p[i], f)) {
                Ꮡt.Errorf("Log1p(%g) = %g, want %g"u8, aΔ1, f, log1p[i]);
            }
        }
    }
    var a = 9.0D;
    {
        var f = Log1p(a); if (f != Ln10) {
            Ꮡt.Errorf("Log1p(%g) = %g, want %g"u8, a, f, (float64)(Ln10));
        }
    }
    for (nint i = 0; i < len(vflogSC); i++) {
        {
            var f = Log1p(vflog1pSC[i]); if (!alike(log1pSC[i], f)) {
                Ꮡt.Errorf("Log1p(%g) = %g, want %g"u8, vflog1pSC[i], f, log1pSC[i]);
            }
        }
    }
}

public static void TestLog2(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        var a = Abs(vf[i]);
        {
            var f = Log2(a); if (!veryclose(log2[i], f)) {
                Ꮡt.Errorf("Log2(%g) = %g, want %g"u8, a, f, log2[i]);
            }
        }
    }
    {
        var f = Log2(E); if (f != Log2E) {
            Ꮡt.Errorf("Log2(%g) = %g, want %g"u8, (float64)(E), f, (float64)(Log2E));
        }
    }
    for (nint i = 0; i < len(vflogSC); i++) {
        {
            var f = Log2(vflogSC[i]); if (!alike(logSC[i], f)) {
                Ꮡt.Errorf("Log2(%g) = %g, want %g"u8, vflogSC[i], f, logSC[i]);
            }
        }
    }
    for (nint i = -1074; i <= 1023; i++) {
        var f = Ldexp(1D, i);
        var l = Log2(f);
        if (l != (float64)i) {
            Ꮡt.Errorf("Log2(2**%d) = %g, want %d"u8, i, l, i);
        }
    }
}

public static void TestModf(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        {
            var (f, g) = Modf(vf[i]); if (!veryclose(modf[i][0], f) || !veryclose(modf[i][1], g)) {
                Ꮡt.Errorf("Modf(%g) = %g, %g, want %g, %g"u8, vf[i], f, g, modf[i][0], modf[i][1]);
            }
        }
    }
    for (nint i = 0; i < len(vfmodfSC); i++) {
        {
            var (f, g) = Modf(vfmodfSC[i]); if (!alike(modfSC[i][0], f) || !alike(modfSC[i][1], g)) {
                Ꮡt.Errorf("Modf(%g) = %g, %g, want %g, %g"u8, vfmodfSC[i], f, g, modfSC[i][0], modfSC[i][1]);
            }
        }
    }
}

public static void TestNextafter32(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        var vfi = (float32)vf[i];
        {
            var f = Nextafter32(vfi, 10F); if (nextafter32[i] != f) {
                Ꮡt.Errorf("Nextafter32(%g, %g) = %g want %g"u8, vfi, 10.0D, f, nextafter32[i]);
            }
        }
    }
    for (nint i = 0; i < len(vfnextafter32SC); i++) {
        {
            var f = Nextafter32(vfnextafter32SC[i][0], vfnextafter32SC[i][1]); if (!alike((float64)nextafter32SC[i], (float64)f)) {
                Ꮡt.Errorf("Nextafter32(%g, %g) = %g want %g"u8, vfnextafter32SC[i][0], vfnextafter32SC[i][1], f, nextafter32SC[i]);
            }
        }
    }
}

public static void TestNextafter64(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        {
            var f = Nextafter(vf[i], 10D); if (nextafter64[i] != f) {
                Ꮡt.Errorf("Nextafter64(%g, %g) = %g want %g"u8, vf[i], 10.0D, f, nextafter64[i]);
            }
        }
    }
    for (nint i = 0; i < len(vfnextafter64SC); i++) {
        {
            var f = Nextafter(vfnextafter64SC[i][0], vfnextafter64SC[i][1]); if (!alike(nextafter64SC[i], f)) {
                Ꮡt.Errorf("Nextafter64(%g, %g) = %g want %g"u8, vfnextafter64SC[i][0], vfnextafter64SC[i][1], f, nextafter64SC[i]);
            }
        }
    }
}

public static void TestPow(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        {
            var f = Pow(10D, vf[i]); if (!close(pow[i], f)) {
                Ꮡt.Errorf("Pow(10, %g) = %g, want %g"u8, vf[i], f, pow[i]);
            }
        }
    }
    for (nint i = 0; i < len(vfpowSC); i++) {
        {
            var f = Pow(vfpowSC[i][0], vfpowSC[i][1]); if (!alike(powSC[i], f)) {
                Ꮡt.Errorf("Pow(%g, %g) = %g, want %g"u8, vfpowSC[i][0], vfpowSC[i][1], f, powSC[i]);
            }
        }
    }
}

public static void TestPow10(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vfpow10SC); i++) {
        {
            var f = Pow10(vfpow10SC[i]); if (!alike(pow10SC[i], f)) {
                Ꮡt.Errorf("Pow10(%d) = %g, want %g"u8, vfpow10SC[i], f, pow10SC[i]);
            }
        }
    }
}

public static void TestRemainder(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        {
            var f = Remainder(10D, vf[i]); if (remainder[i] != f) {
                Ꮡt.Errorf("Remainder(10, %g) = %g, want %g"u8, vf[i], f, remainder[i]);
            }
        }
    }
    for (nint i = 0; i < len(vffmodSC); i++) {
        {
            var f = Remainder(vffmodSC[i][0], vffmodSC[i][1]); if (!alike(fmodSC[i], f)) {
                Ꮡt.Errorf("Remainder(%g, %g) = %g, want %g"u8, vffmodSC[i][0], vffmodSC[i][1], f, fmodSC[i]);
            }
        }
    }
    // verify precision of result for extreme inputs
    {
        var f = Remainder(5.9790119248836734e+200D, 1.1258465975523544D); if (-0.4810497673014966D != f) {
            Ꮡt.Errorf("Remainder(5.9790119248836734e+200, 1.1258465975523544) = %g, want -0.4810497673014966"u8, f);
        }
    }
    // verify that sign is correct when r == 0.
    void test(float64 x, float64 y) {
        {
            var r = Remainder(x, y); if (r == 0D && Signbit(r) != Signbit(x)) {
                Ꮡt.Errorf("Remainder(x=%f, y=%f) = %f, sign of (zero) result should agree with sign of x"u8, x, y, r);
            }
        }
    }
    for (var x = 0.0D; x <= 3.0D; x += 1D) {
        for (var y = 1.0D; y <= 3.0D; y += 1D) {
            test(x, y);
            test(x, -y);
            test(-x, y);
            test(-x, -y);
        }
    }
}

public static void TestRound(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        {
            var f = Round(vf[i]); if (!alike(round[i], f)) {
                Ꮡt.Errorf("Round(%g) = %g, want %g"u8, vf[i], f, round[i]);
            }
        }
    }
    for (nint i = 0; i < len(vfroundSC); i++) {
        {
            var f = Round(vfroundSC[i][0]); if (!alike(vfroundSC[i][1], f)) {
                Ꮡt.Errorf("Round(%g) = %g, want %g"u8, vfroundSC[i][0], f, vfroundSC[i][1]);
            }
        }
    }
}

public static void TestRoundToEven(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        {
            var f = RoundToEven(vf[i]); if (!alike(round[i], f)) {
                Ꮡt.Errorf("RoundToEven(%g) = %g, want %g"u8, vf[i], f, round[i]);
            }
        }
    }
    for (nint i = 0; i < len(vfroundEvenSC); i++) {
        {
            var f = RoundToEven(vfroundEvenSC[i][0]); if (!alike(vfroundEvenSC[i][1], f)) {
                Ꮡt.Errorf("RoundToEven(%g) = %g, want %g"u8, vfroundEvenSC[i][0], f, vfroundEvenSC[i][1]);
            }
        }
    }
}

public static void TestSignbit(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    for (nint i = 0; i < len(vf); i++) {
        {
            var f = Signbit(vf[i]); if (signbit[i] != f) {
                Ꮡt.Errorf("Signbit(%g) = %t, want %t"u8, vf[i], f, signbit[i]);
            }
        }
    }
    for (nint i = 0; i < len(vfsignbitSC); i++) {
        {
            var f = Signbit(vfsignbitSC[i]); if (signbitSC[i] != f) {
                Ꮡt.Errorf("Signbit(%g) = %t, want %t"u8, vfsignbitSC[i], f, signbitSC[i]);
            }
        }
    }
}

public static void TestSin(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        {
            var f = Sin(vf[i]); if (!veryclose(sin[i], f)) {
                Ꮡt.Errorf("Sin(%g) = %g, want %g"u8, vf[i], f, sin[i]);
            }
        }
    }
    for (nint i = 0; i < len(vfsinSC); i++) {
        {
            var f = Sin(vfsinSC[i]); if (!alike(sinSC[i], f)) {
                Ꮡt.Errorf("Sin(%g) = %g, want %g"u8, vfsinSC[i], f, sinSC[i]);
            }
        }
    }
}

public static void TestSincos(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        {
            var (s, c) = Sincos(vf[i]); if (!veryclose(sin[i], s) || !veryclose(cos[i], c)) {
                Ꮡt.Errorf("Sincos(%g) = %g, %g want %g, %g"u8, vf[i], s, c, sin[i], cos[i]);
            }
        }
    }
}

public static void TestSinh(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        {
            var f = Sinh(vf[i]); if (!close(sinh[i], f)) {
                Ꮡt.Errorf("Sinh(%g) = %g, want %g"u8, vf[i], f, sinh[i]);
            }
        }
    }
    for (nint i = 0; i < len(vfsinhSC); i++) {
        {
            var f = Sinh(vfsinhSC[i]); if (!alike(sinhSC[i], f)) {
                Ꮡt.Errorf("Sinh(%g) = %g, want %g"u8, vfsinhSC[i], f, sinhSC[i]);
            }
        }
    }
}

public static void TestSqrt(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        var a = Abs(vf[i]);
        {
            var f = math_internal_test_package.SqrtGo(a); if (sqrt[i] != f) {
                Ꮡt.Errorf("SqrtGo(%g) = %g, want %g"u8, a, f, sqrt[i]);
            }
        }
        a = Abs(vf[i]);
        {
            var f = Sqrt(a); if (sqrt[i] != f) {
                Ꮡt.Errorf("Sqrt(%g) = %g, want %g"u8, a, f, sqrt[i]);
            }
        }
    }
    for (nint i = 0; i < len(vfsqrtSC); i++) {
        {
            var f = math_internal_test_package.SqrtGo(vfsqrtSC[i]); if (!alike(sqrtSC[i], f)) {
                Ꮡt.Errorf("SqrtGo(%g) = %g, want %g"u8, vfsqrtSC[i], f, sqrtSC[i]);
            }
        }
        {
            var f = Sqrt(vfsqrtSC[i]); if (!alike(sqrtSC[i], f)) {
                Ꮡt.Errorf("Sqrt(%g) = %g, want %g"u8, vfsqrtSC[i], f, sqrtSC[i]);
            }
        }
    }
}

public static void TestTan(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        {
            var f = Tan(vf[i]); if (!veryclose(tan[i], f)) {
                Ꮡt.Errorf("Tan(%g) = %g, want %g"u8, vf[i], f, tan[i]);
            }
        }
    }
    // same special cases as Sin
    for (nint i = 0; i < len(vfsinSC); i++) {
        {
            var f = Tan(vfsinSC[i]); if (!alike(sinSC[i], f)) {
                Ꮡt.Errorf("Tan(%g) = %g, want %g"u8, vfsinSC[i], f, sinSC[i]);
            }
        }
    }
}

public static void TestTanh(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        {
            var f = Tanh(vf[i]); if (!veryclose(tanh[i], f)) {
                Ꮡt.Errorf("Tanh(%g) = %g, want %g"u8, vf[i], f, tanh[i]);
            }
        }
    }
    for (nint i = 0; i < len(vftanhSC); i++) {
        {
            var f = Tanh(vftanhSC[i]); if (!alike(tanhSC[i], f)) {
                Ꮡt.Errorf("Tanh(%g) = %g, want %g"u8, vftanhSC[i], f, tanhSC[i]);
            }
        }
    }
}

public static void TestTrunc(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        {
            var f = Trunc(vf[i]); if (!alike(trunc[i], f)) {
                Ꮡt.Errorf("Trunc(%g) = %g, want %g"u8, vf[i], f, trunc[i]);
            }
        }
    }
    for (nint i = 0; i < len(vfceilSC); i++) {
        {
            var f = Trunc(vfceilSC[i]); if (!alike(truncSC[i], f)) {
                Ꮡt.Errorf("Trunc(%g) = %g, want %g"u8, vfceilSC[i], f, truncSC[i]);
            }
        }
    }
}

public static void TestY0(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        var a = Abs(vf[i]);
        {
            var f = Y0(a); if (!close(y0[i], f)) {
                Ꮡt.Errorf("Y0(%g) = %g, want %g"u8, a, f, y0[i]);
            }
        }
    }
    for (nint i = 0; i < len(vfy0SC); i++) {
        {
            var f = Y0(vfy0SC[i]); if (!alike(y0SC[i], f)) {
                Ꮡt.Errorf("Y0(%g) = %g, want %g"u8, vfy0SC[i], f, y0SC[i]);
            }
        }
    }
}

public static void TestY1(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        var a = Abs(vf[i]);
        {
            var f = Y1(a); if (!soclose(y1[i], f, 2e-14D)) {
                Ꮡt.Errorf("Y1(%g) = %g, want %g"u8, a, f, y1[i]);
            }
        }
    }
    for (nint i = 0; i < len(vfy0SC); i++) {
        {
            var f = Y1(vfy0SC[i]); if (!alike(y1SC[i], f)) {
                Ꮡt.Errorf("Y1(%g) = %g, want %g"u8, vfy0SC[i], f, y1SC[i]);
            }
        }
    }
}

public static void TestYn(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(vf); i++) {
        var a = Abs(vf[i]);
        {
            var f = Yn(2, a); if (!close(y2[i], f)) {
                Ꮡt.Errorf("Yn(2, %g) = %g, want %g"u8, a, f, y2[i]);
            }
        }
        {
            var f = Yn(-3, a); if (!close(yM3[i], f)) {
                Ꮡt.Errorf("Yn(-3, %g) = %g, want %g"u8, a, f, yM3[i]);
            }
        }
    }
    for (nint i = 0; i < len(vfy0SC); i++) {
        {
            var f = Yn(2, vfy0SC[i]); if (!alike(y2SC[i], f)) {
                Ꮡt.Errorf("Yn(2, %g) = %g, want %g"u8, vfy0SC[i], f, y2SC[i]);
            }
        }
        {
            var f = Yn(-3, vfy0SC[i]); if (!alike(yM3SC[i], f)) {
                Ꮡt.Errorf("Yn(-3, %g) = %g, want %g"u8, vfy0SC[i], f, yM3SC[i]);
            }
        }
    }
    {
        var f = Yn(0, 0D); if (!alike(Inf(-1), f)) {
            Ꮡt.Errorf("Yn(0, 0) = %g, want %g"u8, f, Inf(-1));
        }
    }
}

public static Func<float64, float64, float64, float64> PortableFMA = FMA;                          // hide call from compiler intrinsic; falls back to portable code

public static void TestFMA(ж<testing.T> Ꮡt) {
    foreach (var (_, c) in fmaC) {
        var got = FMA(c.x, c.y, c.z);
        if (!alike(got, c.want)) {
            Ꮡt.Errorf("FMA(%g,%g,%g) == %g; want %g"u8, c.x, c.y, c.z, got, c.want);
        }
        got = PortableFMA(c.x, c.y, c.z);
        if (!alike(got, c.want)) {
            Ꮡt.Errorf("PortableFMA(%g,%g,%g) == %g; want %g"u8, c.x, c.y, c.z, got, c.want);
        }
    }
}

//go:noinline
internal static partial float64 fmsub(float64 x, float64 y, float64 z) {
    return FMA(x, y, -z);
}

//go:noinline
internal static partial float64 fnmsub(float64 x, float64 y, float64 z) {
    return FMA(-x, y, z);
}

//go:noinline
internal static partial float64 fnmadd(float64 x, float64 y, float64 z) {
    return FMA(-x, y, -z);
}

public static void TestFMANegativeArgs(ж<testing.T> Ꮡt) {
    // Some architectures have instructions for fused multiply-subtract and
    // also negated variants of fused multiply-add and subtract. This test
    // aims to check that the optimizations that generate those instructions
    // are applied correctly, if they exist.
    foreach (var (_, c) in fmaC) {
        var want = PortableFMA(c.x, c.y, -c.z);
        var got = fmsub(c.x, c.y, c.z);
        if (!alike(got, want)) {
            Ꮡt.Errorf("FMA(%g, %g, -(%g)) == %g, want %g"u8, c.x, c.y, c.z, got, want);
        }
        want = PortableFMA(-c.x, c.y, c.z);
        got = fnmsub(c.x, c.y, c.z);
        if (!alike(got, want)) {
            Ꮡt.Errorf("FMA(-(%g), %g, %g) == %g, want %g"u8, c.x, c.y, c.z, got, want);
        }
        want = PortableFMA(-c.x, c.y, -c.z);
        got = fnmadd(c.x, c.y, c.z);
        if (!alike(got, want)) {
            Ꮡt.Errorf("FMA(-(%g), %g, -(%g)) == %g, want %g"u8, c.x, c.y, c.z, got, want);
        }
    }
}

// Check that math functions of high angle values
// return accurate results. [Since (vf[i] + large) - large != vf[i],
// testing for Trig(vf[i] + large) == Trig(vf[i]), where large is
// a multiple of 2*Pi, is misleading.]
public static void TestLargeCos(ж<testing.T> Ꮡt) {
    var large = /* 100000 * Pi */ 314159.26535897935D;
    for (nint i = 0; i < len(vf); i++) {
        var f1 = cosLarge[i];
        var f2 = Cos(vf[i] + large);
        if (!close(f1, f2)) {
            Ꮡt.Errorf("Cos(%g) = %g, want %g"u8, vf[i] + large, f2, f1);
        }
    }
}

public static void TestLargeSin(ж<testing.T> Ꮡt) {
    var large = /* 100000 * Pi */ 314159.26535897935D;
    for (nint i = 0; i < len(vf); i++) {
        var f1 = sinLarge[i];
        var f2 = Sin(vf[i] + large);
        if (!close(f1, f2)) {
            Ꮡt.Errorf("Sin(%g) = %g, want %g"u8, vf[i] + large, f2, f1);
        }
    }
}

public static void TestLargeSincos(ж<testing.T> Ꮡt) {
    var large = /* 100000 * Pi */ 314159.26535897935D;
    for (nint i = 0; i < len(vf); i++) {
        var (f1, g1) = (sinLarge[i], cosLarge[i]);
        var (f2, g2) = Sincos(vf[i] + large);
        if (!close(f1, f2) || !close(g1, g2)) {
            Ꮡt.Errorf("Sincos(%g) = %g, %g, want %g, %g"u8, vf[i] + large, f2, g2, f1, g1);
        }
    }
}

public static void TestLargeTan(ж<testing.T> Ꮡt) {
    var large = /* 100000 * Pi */ 314159.26535897935D;
    for (nint i = 0; i < len(vf); i++) {
        var f1 = tanLarge[i];
        var f2 = Tan(vf[i] + large);
        if (!close(f1, f2)) {
            Ꮡt.Errorf("Tan(%g) = %g, want %g"u8, vf[i] + large, f2, f1);
        }
    }
}

// Check that trigReduce matches the standard reduction results for input values
// below reduceThreshold.
public static void TestTrigReduce(ж<testing.T> Ꮡt) {
    var inputs = new slice<float64>(len(vf));
    // all of the standard inputs
    copy(inputs, vf);
    // all of the large inputs
    var large = /* 100000 * Pi */ 314159.26535897935D;
    foreach (var (_, v) in vf) {
        inputs = append(inputs, v + large);
    }
    // Also test some special inputs, Pi and right below the reduceThreshold
    inputs = append(inputs, (float64)(Pi), Nextafter(math_internal_test_package.ReduceThreshold, 0D));
    foreach (var (_, x) in inputs) {
        // reduce the value to compare
        var (j, z) = math_internal_test_package.TrigReduce(x);
        var xred = (float64)j * /* (Pi / 4) */ 0.7853981633974483D + z;
        {
            var (fΔ1, fredΔ1) = (Sin(x), Sin(xred)); if (!close(fΔ1, fredΔ1)) {
                Ꮡt.Errorf("Sin(trigReduce(%g)) != Sin(%g), got %g, want %g"u8, x, x, fredΔ1, fΔ1);
            }
        }
        {
            var (fΔ2, fredΔ2) = (Cos(x), Cos(xred)); if (!close(fΔ2, fredΔ2)) {
                Ꮡt.Errorf("Cos(trigReduce(%g)) != Cos(%g), got %g, want %g"u8, x, x, fredΔ2, fΔ2);
            }
        }
        {
            var (fΔ3, fredΔ3) = (Tan(x), Tan(xred)); if (!close(fΔ3, fredΔ3)) {
                Ꮡt.Errorf(" Tan(trigReduce(%g)) != Tan(%g), got %g, want %g"u8, x, x, fredΔ3, fΔ3);
            }
        }
        var (f, g) = Sincos(x);
        var (fred, gred) = Sincos(xred);
        if (!close(f, fred) || !close(g, gred)) {
            Ꮡt.Errorf(" Sincos(trigReduce(%g)) != Sincos(%g), got %g, %g, want %g, %g"u8, x, x, fred, gred, f, g);
        }
    }
}

// Check that math constants are accepted by compiler
// and have right value (assumes strconv.ParseFloat works).
// https://golang.org/issue/201
partial struct floatTest {
    internal any val;
    internal @string name;
    internal @string str;
}

internal static slice<floatTest> floatTests = new floatTest[]{
    new((float64)MaxFloat64, "MaxFloat64"u8, "1.7976931348623157e+308"u8),
    new((float64)SmallestNonzeroFloat64, "SmallestNonzeroFloat64"u8, "5e-324"u8),
    new((float32)MaxFloat32, "MaxFloat32"u8, "3.4028235e+38"u8),
    new((float32)SmallestNonzeroFloat32, "SmallestNonzeroFloat32"u8, "1e-45"u8)
}.slice();

public static void TestFloatMinMax(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in floatTests) {
        @string s = fmt.Sprint(tt.val);
        if (s != tt.str) {
            Ꮡt.Errorf("Sprint(%v) = %s, want %s"u8, tt.name, s, tt.str);
        }
    }
}

public static void TestFloatMinima(ж<testing.T> Ꮡt) {
    {
        var q = /* SmallestNonzeroFloat32 / 2 */ 0F; if (q != 0F) {
            Ꮡt.Errorf("float32(SmallestNonzeroFloat32 / 2) = %g, want 0"u8, q);
        }
    }
    {
        var q = /* SmallestNonzeroFloat64 / 2 */ 0D; if (q != 0D) {
            Ꮡt.Errorf("float64(SmallestNonzeroFloat64 / 2) = %g, want 0"u8, q);
        }
    }
}

internal static Func<float64, float64> indirectSqrt = Sqrt;

// TestFloat32Sqrt checks the correctness of the float32 square root optimization result.
public static void TestFloat32Sqrt(ж<testing.T> Ꮡt) {
    foreach (var (_, v) in sqrt32) {
        var want = (float32)indirectSqrt((float64)v);
        var got = (float32)Sqrt((float64)v);
        if (IsNaN((float64)want)) {
            if (!IsNaN((float64)got)) {
                Ꮡt.Errorf("got=%#v want=NaN, v=%#v"u8, got, v);
            }
            continue;
        }
        if (got != want) {
            Ꮡt.Errorf("got=%#v want=%#v, v=%#v"u8, got, want, v);
        }
    }
}

// Benchmarks

// Global exported variables are used to store the
// return values of functions measured in the benchmarks.
// Storing the results in these variables prevents the compiler
// from completely optimizing the benchmarked functions away.
public static nint GlobalI;

public static bool GlobalB;

public static float64 GlobalF;

public static void BenchmarkAcos(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Acos(.5D);
    }
    GlobalF = x;
}

public static void BenchmarkAcosh(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Acosh(1.5D);
    }
    GlobalF = x;
}

public static void BenchmarkAsin(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Asin(.5D);
    }
    GlobalF = x;
}

public static void BenchmarkAsinh(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Asinh(.5D);
    }
    GlobalF = x;
}

public static void BenchmarkAtan(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Atan(.5D);
    }
    GlobalF = x;
}

public static void BenchmarkAtanh(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Atanh(.5D);
    }
    GlobalF = x;
}

public static void BenchmarkAtan2(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Atan2(.5D, 1D);
    }
    GlobalF = x;
}

public static void BenchmarkCbrt(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Cbrt(10D);
    }
    GlobalF = x;
}

public static void BenchmarkCeil(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Ceil(.5D);
    }
    GlobalF = x;
}

internal static float64 copysignNeg = -1.0D;

public static void BenchmarkCopysign(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Copysign(.5D, copysignNeg);
    }
    GlobalF = x;
}

public static void BenchmarkCos(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Cos(.5D);
    }
    GlobalF = x;
}

public static void BenchmarkCosh(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Cosh(2.5D);
    }
    GlobalF = x;
}

public static void BenchmarkErf(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Erf(.5D);
    }
    GlobalF = x;
}

public static void BenchmarkErfc(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Erfc(.5D);
    }
    GlobalF = x;
}

public static void BenchmarkErfinv(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Erfinv(.5D);
    }
    GlobalF = x;
}

public static void BenchmarkErfcinv(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Erfcinv(.5D);
    }
    GlobalF = x;
}

public static void BenchmarkExp(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Exp(.5D);
    }
    GlobalF = x;
}

public static void BenchmarkExpGo(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = math_internal_test_package.ExpGo(.5D);
    }
    GlobalF = x;
}

public static void BenchmarkExpm1(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Expm1(.5D);
    }
    GlobalF = x;
}

public static void BenchmarkExp2(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Exp2(.5D);
    }
    GlobalF = x;
}

public static void BenchmarkExp2Go(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = math_internal_test_package.Exp2Go(.5D);
    }
    GlobalF = x;
}

internal static float64 absPos = .5D;

public static void BenchmarkAbs(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Abs(absPos);
    }
    GlobalF = x;
}

public static void BenchmarkDim(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Dim(GlobalF, x);
    }
    GlobalF = x;
}

public static void BenchmarkFloor(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Floor(.5D);
    }
    GlobalF = x;
}

public static void BenchmarkMax(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Max(10D, 3D);
    }
    GlobalF = x;
}

public static void BenchmarkMin(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Min(10D, 3D);
    }
    GlobalF = x;
}

public static void BenchmarkMod(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Mod(10D, 3D);
    }
    GlobalF = x;
}

public static void BenchmarkFrexp(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    nint y = 0;
    for (nint i = 0; i < b.N; i++) {
        (x, y) = Frexp(8D);
    }
    GlobalF = x;
    GlobalI = y;
}

public static void BenchmarkGamma(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Gamma(2.5D);
    }
    GlobalF = x;
}

public static void BenchmarkHypot(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Hypot(3D, 4D);
    }
    GlobalF = x;
}

public static void BenchmarkHypotGo(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = math_internal_test_package.HypotGo(3D, 4D);
    }
    GlobalF = x;
}

public static void BenchmarkIlogb(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint x = 0;
    for (nint i = 0; i < b.N; i++) {
        x = Ilogb(.5D);
    }
    GlobalI = x;
}

public static void BenchmarkJ0(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = J0(2.5D);
    }
    GlobalF = x;
}

public static void BenchmarkJ1(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = J1(2.5D);
    }
    GlobalF = x;
}

public static void BenchmarkJn(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Jn(2, 2.5D);
    }
    GlobalF = x;
}

public static void BenchmarkLdexp(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Ldexp(.5D, 2);
    }
    GlobalF = x;
}

public static void BenchmarkLgamma(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    nint y = 0;
    for (nint i = 0; i < b.N; i++) {
        (x, y) = Lgamma(2.5D);
    }
    GlobalF = x;
    GlobalI = y;
}

public static void BenchmarkLog(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Log(.5D);
    }
    GlobalF = x;
}

public static void BenchmarkLogb(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Logb(.5D);
    }
    GlobalF = x;
}

public static void BenchmarkLog1p(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Log1p(.5D);
    }
    GlobalF = x;
}

public static void BenchmarkLog10(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Log10(.5D);
    }
    GlobalF = x;
}

public static void BenchmarkLog2(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Log2(.5D);
    }
    GlobalF += x;
}

public static void BenchmarkModf(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    var y = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        (x, y) = Modf(1.5D);
    }
    GlobalF += x;
    GlobalF += y;
}

public static void BenchmarkNextafter32(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = (float32)0.0F;
    for (nint i = 0; i < b.N; i++) {
        x = Nextafter32(.5F, 1F);
    }
    GlobalF = (float64)x;
}

public static void BenchmarkNextafter64(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Nextafter(.5D, 1D);
    }
    GlobalF = x;
}

public static void BenchmarkPowInt(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Pow(2D, 2D);
    }
    GlobalF = x;
}

public static void BenchmarkPowFrac(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Pow(2.5D, 1.5D);
    }
    GlobalF = x;
}

internal static nint pow10pos = (nint)300;

public static void BenchmarkPow10Pos(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Pow10(pow10pos);
    }
    GlobalF = x;
}

internal static nint pow10neg = (nint)(-300);

public static void BenchmarkPow10Neg(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Pow10(pow10neg);
    }
    GlobalF = x;
}

internal static float64 roundNeg = (float64)(-2.5D);

public static void BenchmarkRound(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Round(roundNeg);
    }
    GlobalF = x;
}

public static void BenchmarkRoundToEven(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = RoundToEven(roundNeg);
    }
    GlobalF = x;
}

public static void BenchmarkRemainder(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Remainder(10D, 3D);
    }
    GlobalF = x;
}

internal static float64 signbitPos = 2.5D;

public static void BenchmarkSignbit(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = false;
    for (nint i = 0; i < b.N; i++) {
        x = Signbit(signbitPos);
    }
    GlobalB = x;
}

public static void BenchmarkSin(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Sin(.5D);
    }
    GlobalF = x;
}

public static void BenchmarkSincos(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    var y = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        (x, y) = Sincos(.5D);
    }
    GlobalF += x;
    GlobalF += y;
}

public static void BenchmarkSinh(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Sinh(2.5D);
    }
    GlobalF = x;
}

public static void BenchmarkSqrtIndirect(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var (x, y) = (0.0D, 10.0D);
    var f = Sqrt;
    for (nint i = 0; i < b.N; i++) {
        x += f(y);
    }
    GlobalF = x;
}

public static void BenchmarkSqrtLatency(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 10.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Sqrt(x);
    }
    GlobalF = x;
}

public static void BenchmarkSqrtIndirectLatency(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 10.0D;
    var f = Sqrt;
    for (nint i = 0; i < b.N; i++) {
        x = f(x);
    }
    GlobalF = x;
}

public static void BenchmarkSqrtGoLatency(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 10.0D;
    for (nint i = 0; i < b.N; i++) {
        x = math_internal_test_package.SqrtGo(x);
    }
    GlobalF = x;
}

internal static bool isPrime(nint i) {
    // Yes, this is a dumb way to write this code,
    // but calling Sqrt repeatedly in this way demonstrates
    // the benefit of using a direct SQRT instruction on systems
    // that have one, whereas the obvious loop seems not to
    // demonstrate such a benefit.
    for (nint j = 2; (float64)j <= Sqrt((float64)i); j++) {
        if (rem(i, j) == 0) {
            return false;
        }
    }
    return true;
}

public static void BenchmarkSqrtPrime(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = false;
    for (nint i = 0; i < b.N; i++) {
        x = isPrime(100003);
    }
    GlobalB = x;
}

public static void BenchmarkTan(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Tan(.5D);
    }
    GlobalF = x;
}

public static void BenchmarkTanh(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Tanh(2.5D);
    }
    GlobalF = x;
}

public static void BenchmarkTrunc(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Trunc(.5D);
    }
    GlobalF = x;
}

public static void BenchmarkY0(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Y0(2.5D);
    }
    GlobalF = x;
}

public static void BenchmarkY1(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Y1(2.5D);
    }
    GlobalF = x;
}

public static void BenchmarkYn(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Yn(2, 2.5D);
    }
    GlobalF = x;
}

public static void BenchmarkFloat64bits(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var y = (uint64)0;
    for (nint i = 0; i < b.N; i++) {
        y = Float64bits(roundNeg);
    }
    GlobalI = (nint)y;
}

internal static uint64 roundUint64 = (uint64)5;

public static void BenchmarkFloat64frombits(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = Float64frombits(roundUint64);
    }
    GlobalF = x;
}

internal static float32 roundFloat32 = (float32)(-2.5F);

public static void BenchmarkFloat32bits(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var y = (uint32)0;
    for (nint i = 0; i < b.N; i++) {
        y = Float32bits(roundFloat32);
    }
    GlobalI = (nint)y;
}

internal static uint32 roundUint32 = (uint32)5;

public static void BenchmarkFloat32frombits(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = (float32)0.0F;
    for (nint i = 0; i < b.N; i++) {
        x = Float32frombits(roundUint32);
    }
    GlobalF = (float64)x;
}

public static void BenchmarkFMA(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = 0.0D;
    for (nint i = 0; i < b.N; i++) {
        x = FMA(E, Pi, x);
    }
    GlobalF = x;
}

} // end math_test_package
