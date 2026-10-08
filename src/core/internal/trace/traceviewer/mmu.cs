// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Minimum mutator utilization (MMU) graphing.
// TODO:
//
// In worst window list, show break-down of GC utilization sources
// (STW, assist, etc). Probably requires a different MutatorUtil
// representation.
//
// When a window size is selected, show a second plot of the mutator
// utilization distribution for that window size.
//
// Render plot progressively so rough outline is visible quickly even
// for very complex MUTs. Start by computing just a few window sizes
// and then add more window sizes.
//
// Consider using sampling to compute an approximate MUT. This would
// work by sampling the mutator utilization at randomly selected
// points in time in the trace to build an empirical distribution. We
// could potentially put confidence intervals on these estimates and
// render this progressively as we refine the distributions.
namespace go.@internal.trace;

using json = encoding.json_package;
using fmt = fmt_package;
using trace = go.@internal.trace_package;
using log = log_package;
using math = math_package;
using http = net.http_package;
using strconv = strconv_package;
using strings = strings_package;
using sync = go.sync_package;
using time = time_package;
using encoding;
using go;
using go.@internal;
using io = io_package;
using net;

partial class traceviewer_package {

// type MutatorUtilFunc is a methodless func type — rendered inline as its base delegate

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string modeˢ = "mode"u8;

public static http.HandlerFunc MMUHandlerFunc(slice<Range> ranges, Func<trace.UtilFlags, (slice<slice<trace.MutatorUtil>>, error)> f) {
    var mmu = Ꮡ(new mmu(
        cache: new map<trace.UtilFlags, ж<mmuCacheEntry>>(),
        f: f,
        ranges: ranges
    ));
    var mmuʗ1 = mmu;
    return (http.ResponseWriter w, ж<http.Request> r) => {
        var exprᴛ1 = r.FormValue(modeˢ);
        if (exprᴛ1 == "plot"u8) {
            mmuʗ1.HandlePlot(w, r);
            return;
        }
        if (exprᴛ1 == "details"u8) {
            mmuʗ1.HandleDetails(w, r);
            return;
        }

        http.ServeContent(w, r, ""u8, new time.Time(nil), new strings_ReaderжReadSeeker(strings.NewReader(templMMU)));
    };
}

internal static map<@string, trace.UtilFlags> utilFlagNames = new map<@string, trace.UtilFlags>{
    ["perProc"u8] = trace.UtilPerProc,
    ["stw"u8] = trace.UtilSTW,
    ["background"u8] = trace.UtilBackground,
    ["assist"u8] = trace.UtilAssist,
    ["sweep"u8] = trace.UtilSweep
};

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string flagsˢ = "flags"u8;

internal static trace.UtilFlags requestUtilFlags(ж<http.Request> Ꮡr) {
    trace.UtilFlags flags = default!;
    foreach (var (_, flagStr) in strings.Split(Ꮡr.FormValue(flagsˢ), "|"u8)) {
        flags |= (trace.UtilFlags)(utilFlagNames[flagStr]);
    }
    return flags;
}

partial struct mmuCacheEntry {
    internal sync.Once init;
    internal slice<slice<trace.MutatorUtil>> util;
    internal ж<trace.MMUCurve> mmuCurve;
    internal error err;
}

partial struct mmu {
    internal sync.Mutex mu;
    internal map<trace.UtilFlags, ж<mmuCacheEntry>> cache;
    internal Func<trace.UtilFlags, (slice<slice<trace.MutatorUtil>>, error)> f;
    internal slice<Range> ranges;
}

internal static (slice<slice<trace.MutatorUtil>>, ж<trace.MMUCurve>, error) get(this ж<mmu> Ꮡm, trace.UtilFlags flags) {
    ref var m = ref Ꮡm.DerefOrNull();

    m.mu.Lock();
    var entry = m.cache[flags];
    if (entry == nil) {
        entry = @new<mmuCacheEntry>();
        m.cache[flags] = entry;
    }
    m.mu.Unlock();
    var entryʗ1 = entry;
    entry.of(mmuCacheEntry.Ꮡinit).Do(() => {
        var (util, err) = Ꮡm.Value.f(flags);
        if (err != default!){
            entryʗ1.Value.err = err;
        } else {
            entryʗ1.Value.util = util;
            entryʗ1.Value.mmuCurve = trace.NewMMUCurve(util);
        }
    });
    return ((~entry).util, (~entry).mmuCurve, (~entry).err);
}

// HandlePlot serves the JSON data for the MMU plot.
internal static void HandlePlot(this ж<mmu> Ꮡm, http.ResponseWriter w, ж<http.Request> Ꮡr) {
    var (mu, mmuCurve, err) = Ꮡm.get(requestUtilFlags(Ꮡr));
    if (err != default!) {
        http.Error(w, fmt.Sprintf("failed to produce MMU data: %v"u8, err), http.StatusInternalServerError);
        return;
    }
    slice<float64> quantiles = default!;
    foreach (var (_, flagStr) in strings.Split(Ꮡr.FormValue(flagsˢ), "|"u8)) {
        if (flagStr == "mut"u8) {
            quantiles = new float64[]{0D, /* 1 - .999 */ 0.001D, /* 1 - .99 */ 0.01D, /* 1 - .95 */ 0.05D}.slice();
            break;
        }
    }
    // Find a nice starting point for the plot.
    var xMin = time.ΔSecond;
    while (xMin > 1) {
        {
            var mmu = mmuCurve.MMU(xMin); if (mmu < 0.0001D) {
                break;
            }
        }
        xMin /= 1000;
    }
    // Cover six orders of magnitude.
    var xMax = xMin * 1000000;
    // But no more than the length of the trace.
    var (minEvent, maxEvent) = (mu[0][0].Time, mu[0][len(mu[0]) - 1].Time);
    foreach (var (_, mu1) in mu[1..]) {
        if (mu1[0].Time < minEvent) {
            minEvent = mu1[0].Time;
        }
        if (mu1[len(mu1) - 1].Time > maxEvent) {
            maxEvent = mu1[len(mu1) - 1].Time;
        }
    }
    {
        var maxMax = ((time.Duration)(maxEvent - minEvent)); if (xMax > maxMax) {
            xMax = maxMax;
        }
    }
    // Compute MMU curve.
    var (logMin, logMax) = (math.Log((float64)(int64)xMin), math.Log((float64)(int64)xMax));
    UntypedInt samples = 100;
    var plot = new slice<slice<float64>>(samples);
    for (nint i = 0; i < samples; i++) {
        var window = ((time.Duration)(int64)math.Exp((float64)i / /* (samples - 1) */ 99D * (logMax - logMin) + logMin));
        if (quantiles == default!){
            plot[i] = new slice<float64>(2);
            plot[i][1] = mmuCurve.MMU(window);
        } else {
            plot[i] = new slice<float64>(1 + len(quantiles));
            copy(plot[i][1..], mmuCurve.MUD(window, quantiles));
        }
        plot[i][0] = (float64)(int64)window;
    }
    // Create JSON response.
    err = json.NewEncoder(new http_ResponseWriterᴠWriter(w)).Encode(new map<@string, any>{["xMin"u8] = (int64)xMin, ["xMax"u8] = (int64)xMax, ["quantiles"u8] = quantiles, ["curve"u8] = plot});
    if (err != default!) {
        log.Printf("failed to serialize response: %v"u8, err);
        return;
    }
}

internal static @string templMMU = """
<!doctype html>
<html>
  <head>
    <meta charset="utf-8">
    <script type="text/javascript" src="https://www.gstatic.com/charts/loader.js"></script>
    <script type="text/javascript" src="https://ajax.googleapis.com/ajax/libs/jquery/3.2.1/jquery.min.js"></script>
    <script type="text/javascript">
      google.charts.load('current', {'packages':['corechart']});
      var chartsReady = false;
      google.charts.setOnLoadCallback(function() { chartsReady = true; refreshChart(); });

      var chart;
      var curve;

      function niceDuration(ns) {
          if (ns < 1e3) { return ns + 'ns'; }
          else if (ns < 1e6) { return ns / 1e3 + 'µs'; }
          else if (ns < 1e9) { return ns / 1e6 + 'ms'; }
          else { return ns / 1e9 + 's'; }
      }

      function niceQuantile(q) {
        return 'p' + q*100;
      }

      function mmuFlags() {
        var flags = "";
        $("#options input").each(function(i, elt) {
          if (elt.checked)
            flags += "|" + elt.id;
        });
        return flags.substr(1);
      }

      function refreshChart() {
        if (!chartsReady) return;
        var container = $('#mmu_chart');
        container.css('opacity', '.5');
        refreshChart.count++;
        var seq = refreshChart.count;
        $.getJSON('?mode=plot&flags=' + mmuFlags())
         .fail(function(xhr, status, error) {
           alert('failed to load plot: ' + status);
         })
         .done(function(result) {
           if (refreshChart.count === seq)
             drawChart(result);
         });
      }
      refreshChart.count = 0;

      function drawChart(plotData) {
        curve = plotData.curve;
        var data = new google.visualization.DataTable();
        data.addColumn('number', 'Window duration');
        data.addColumn('number', 'Minimum mutator utilization');
        if (plotData.quantiles) {
          for (var i = 1; i < plotData.quantiles.length; i++) {
            data.addColumn('number', niceQuantile(1 - plotData.quantiles[i]) + ' MU');
          }
        }
        data.addRows(curve);
        for (var i = 0; i < curve.length; i++) {
          data.setFormattedValue(i, 0, niceDuration(curve[i][0]));
        }

        var options = {
          chart: {
            title: 'Minimum mutator utilization',
          },
          hAxis: {
            title: 'Window duration',
            scaleType: 'log',
            ticks: [],
          },
          vAxis: {
            title: 'Minimum mutator utilization',
            minValue: 0.0,
            maxValue: 1.0,
          },
          legend: { position: 'none' },
          focusTarget: 'category',
          width: 900,
          height: 500,
          chartArea: { width: '80%', height: '80%' },
        };
        for (var v = plotData.xMin; v <= plotData.xMax; v *= 10) {
          options.hAxis.ticks.push({v:v, f:niceDuration(v)});
        }
        if (plotData.quantiles) {
          options.vAxis.title = 'Mutator utilization';
          options.legend.position = 'in';
        }

        var container = $('#mmu_chart');
        container.empty();
        container.css('opacity', '');
        chart = new google.visualization.LineChart(container[0]);
        chart = new google.visualization.LineChart(document.getElementById('mmu_chart'));
        chart.draw(data, options);

        google.visualization.events.addListener(chart, 'select', selectHandler);
        $('#details').empty();
      }

      function selectHandler() {
        var items = chart.getSelection();
        if (items.length === 0) {
          return;
        }
        var details = $('#details');
        details.empty();
        var windowNS = curve[items[0].row][0];
        var url = '?mode=details&window=' + windowNS + '&flags=' + mmuFlags();
        $.getJSON(url)
         .fail(function(xhr, status, error) {
            details.text(status + ': ' + url + ' could not be loaded');
         })
         .done(function(worst) {
            details.text('Lowest mutator utilization in ' + niceDuration(windowNS) + ' windows:');
            for (var i = 0; i < worst.length; i++) {
              details.append($('<br>'));
              var text = worst[i].MutatorUtil.toFixed(3) + ' at time ' + niceDuration(worst[i].Time);
              details.append($('<a/>').text(text).attr('href', worst[i].URL));
            }
         });
      }

      $.when($.ready).then(function() {
        $("#options input").click(refreshChart);
      });
    </script>
    <style>
      .help {
        display: inline-block;
        position: relative;
        width: 1em;
        height: 1em;
        border-radius: 50%;
        color: #fff;
        background: #555;
        text-align: center;
        cursor: help;
      }
      .help > span {
        display: none;
      }
      .help:hover > span {
        display: block;
        position: absolute;
        left: 1.1em;
        top: 1.1em;
        background: #555;
        text-align: left;
        width: 20em;
        padding: 0.5em;
        border-radius: 0.5em;
        z-index: 5;
      }
    </style>
  </head>
  <body>
    <div style="position: relative">
      <div id="mmu_chart" style="width: 900px; height: 500px; display: inline-block; vertical-align: top">Loading plot...</div>
      <div id="options" style="display: inline-block; vertical-align: top">
        <p>
          <b>View</b><br>
          <input type="radio" name="view" id="system" checked><label for="system">System</label>
          <span class="help">?<span>Consider whole system utilization. For example, if one of four procs is available to the mutator, mutator utilization will be 0.25. This is the standard definition of an MMU.</span></span><br>
          <input type="radio" name="view" id="perProc"><label for="perProc">Per-goroutine</label>
          <span class="help">?<span>Consider per-goroutine utilization. When even one goroutine is interrupted by GC, mutator utilization is 0.</span></span><br>
        </p>
        <p>
          <b>Include</b><br>
          <input type="checkbox" id="stw" checked><label for="stw">STW</label>
          <span class="help">?<span>Stop-the-world stops all goroutines simultaneously.</span></span><br>
          <input type="checkbox" id="background" checked><label for="background">Background workers</label>
          <span class="help">?<span>Background workers are GC-specific goroutines. 25% of the CPU is dedicated to background workers during GC.</span></span><br>
          <input type="checkbox" id="assist" checked><label for="assist">Mark assist</label>
          <span class="help">?<span>Mark assists are performed by allocation to prevent the mutator from outpacing GC.</span></span><br>
          <input type="checkbox" id="sweep"><label for="sweep">Sweep</label>
          <span class="help">?<span>Sweep reclaims unused memory between GCs. (Enabling this may be very slow.).</span></span><br>
        </p>
        <p>
          <b>Display</b><br>
          <input type="checkbox" id="mut"><label for="mut">Show percentiles</label>
          <span class="help">?<span>Display percentile mutator utilization in addition to minimum. E.g., p99 MU drops the worst 1% of windows.</span></span><br>
        </p>
      </div>
    </div>
    <div id="details">Select a point for details.</div>
  </body>
</html>

"""u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string windowˢ = "window"u8;

// HandleDetails serves details of an MMU graph at a particular window.
internal static void HandleDetails(this ж<mmu> Ꮡm, http.ResponseWriter w, ж<http.Request> Ꮡr) {
    ref var m = ref Ꮡm.DerefOrNull();

    var (_, mmuCurve, err) = Ꮡm.get(requestUtilFlags(Ꮡr));
    if (err != default!) {
        http.Error(w, fmt.Sprintf("failed to produce MMU data: %v"u8, err), http.StatusInternalServerError);
        return;
    }
    @string windowStr = Ꮡr.FormValue(windowˢ);
    (var window, err) = strconv.ParseUint(windowStr, 10, 64);
    if (err != default!) {
        http.Error(w, fmt.Sprintf("failed to parse window parameter %q: %v"u8, windowStr, err), http.StatusBadRequest);
        return;
    }
    var worst = mmuCurve.Examples(((time.Duration)(int64)window), 10);
    // Construct a link for each window.
    slice<linkedUtilWindow> links = default!;
    foreach (var (_, ui) in worst) {
        links = append(links, m.newLinkedUtilWindow(ui, ((time.Duration)(int64)window)));
    }
    err = json.NewEncoder(new http_ResponseWriterᴠWriter(w)).Encode(links);
    if (err != default!) {
        log.Printf("failed to serialize trace: %v"u8, err);
        return;
    }
}

partial struct linkedUtilWindow {
    public partial ref go.@internal.trace_package.UtilWindow UtilWindow { get; }
    public @string URL;
}

internal static linkedUtilWindow newLinkedUtilWindow(this ref mmu m, trace.UtilWindow ui, time.Duration window) {
    // Find the range containing this window.
    Range r = default!;
    foreach (var (_, vᴛ1) in m.ranges) {
        r = vᴛ1;

        if (r.EndTime > ui.Time) {
            break;
        }
    }
    return new linkedUtilWindow(ui, fmt.Sprintf("%s#%v:%v"u8, r.URL(ViewProc), (float64)ui.Time / 1e6D, (float64)(ui.Time + (int64)window) / 1e6D));
}

} // end traceviewer_package
