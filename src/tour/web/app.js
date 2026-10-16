(() => {
  "use strict";

  const frame = document.querySelector("#tour-frame");
  const workspace = document.querySelector("#workspace");
  const divider = document.querySelector("#column-divider");
  const convertButton = document.querySelector("#convert-button");
  const runButton = document.querySelector("#run-button");
  const syncRunCheckbox = document.querySelector("#sync-run");
  const codeView = document.querySelector("#code-view code");
  const packageInfoView = document.querySelector("#package-info-view code");
  const projectView = document.querySelector("#project-view code");
  const sourceTabs = document.querySelector(".source-tabs");
  const outputTabs = document.querySelector(".output-tabs");
  const runtimeSelect = document.querySelector("#runtime-mode");
  const outputScroll = document.querySelector("#output-scroll");
  const outputView = document.querySelector("#output-view");
  const outputTiming = document.querySelector("#output-timing");
  const lessonLabel = document.querySelector("#lesson-label");
  const conversionStatus = document.querySelector("#conversion-status");
  const connectionDot = document.querySelector("#connection-dot");
  const connectionLabel = document.querySelector("#connection-label");
  const warmupStatus = document.querySelector("#warmup-status");
  const solutionsLink = document.querySelector("#solutions-link");

  let goSource = "";
  let runtimeMode = runtimeSelect.value;
  let runtimeReady = false;
  let pagePath = "";
  let conversionID = "";
  let dirty = false;
  let converting = false;
  let running = false;
  let convertController = null;
  let runController = null;
  // The page's request for the current conversion's build: { conversionId, controller, promise,
  // settled, stage }. The server starts that build when it saves the conversion, so the page asks
  // for it at once, shows it in the Build tab, and Run waits on (or reuses) this same request.
  let buildRequest = null;
  let autoConvertTimer = null;
  let activeOutput = "transpile";
  let synchronizedRunPending = false;
  let synchronizedRunProcessing = false;
  let warmupState = "";
  let warmupPoll = null;
  let warmupReadyTimer = null;

  // Run with Go is on unless this browser turned it off: only a stored "false" unchecks it.
  const syncRunStorageKey = "go2cs-run-with-go";
  syncRunCheckbox.checked = true;
  try {
    syncRunCheckbox.checked = localStorage.getItem(syncRunStorageKey) !== "false";
  } catch {
    // The option still works for this page when storage is unavailable.
  }

  // Stream tags shared with the stage transcripts the server sends.
  const outputStdout = "stdout";
  const outputStderr = "stderr";
  const outputSystem = "system";

  const outputs = {
    transpile: idleStage("transpile", "Open a Tour page to convert its Go code."),
    build: idleStage("build", "Run the converted project to see build output."),
    run: idleStage("run", "Run the converted project to see .NET output.")
  };

  function idleStage(id, message) {
    return { id, status: "idle", output: message, durationMs: 0 };
  }

  // A stage this page produces itself is tagged like one the server sends, so a
  // kill reads as a notice and a failed request reads as an error.
  function taggedStage(id, status, kind, message) {
    return { id, status, output: message, segments: [{ kind, text: message }], durationMs: 0 };
  }

  // A convert request can end before the converter ever runs -- building go2cs is its own stage,
  // and on a slow host that build is what runs out of time -- so the status line names the stage
  // the server reported and says whether it failed or was never measured. Reading "Transpile
  // failed" for a build that never produced a converter blames the conversion for work the
  // converter was not present for.
  function stageFailureMessage(stage) {
    const label = stage?.label || "Transpile";
    if (stage?.timedOut) return `${label} timed out`;
    if (stage?.status === "killed") return `${label} killed`;
    return `${label} failed`;
  }

  function setConnection(online, text) {
    connectionDot.className = online ? "online" : "offline";
    connectionLabel.textContent = text;
  }

  // The server owns the header link, so clearing -solutions-url removes it here.
  function setSolutionsLink(solutions) {
    const url = solutions?.url || "";
    solutionsLink.hidden = !url;
    if (!url) {
      solutionsLink.removeAttribute("href");
      return;
    }
    solutionsLink.href = url;
    solutionsLink.textContent = solutions.text || url;
  }

  // The server warms .NET up at start. While it does, the header says so and the page asks again
  // every few seconds; once it ends, a short ".NET ready" replaces the notice. A failed or skipped
  // warmup shows nothing: the first lesson simply pays the cold start.
  function setWarmup(warmup) {
    const state = warmup?.state || "";
    const previous = warmupState;
    warmupState = state;
    clearTimeout(warmupPoll);
    warmupPoll = null;

    if (state === "running") {
      clearTimeout(warmupReadyTimer);
      warmupStatus.textContent = "Warming up .NET...";
      warmupStatus.hidden = false;
      warmupPoll = setTimeout(checkHealth, 3000);
      return;
    }
    if (state === "ready" && previous === "running") {
      warmupStatus.textContent = ".NET ready";
      warmupStatus.hidden = false;
      clearTimeout(warmupReadyTimer);
      warmupReadyTimer = setTimeout(() => { warmupStatus.hidden = true; }, 4000);
      return;
    }
    if (state !== "ready") warmupStatus.hidden = true;
  }

  async function checkHealth() {
    try {
      const response = await fetch("/api/health");
      const health = await response.json();
      setConnection(health.tour, health.tour ? "Tour of Go connected" : "Tour of Go offline");
      setSolutionsLink(health.solutions);
      setWarmup(health.warmup);
      for (const option of runtimeSelect.options) {
        const runtime = health.runtimes?.find(candidate => candidate.value === option.value);
        if (!runtime) continue;
        option.disabled = !runtime.available;
        option.title = runtime.detail || runtime.label;
      }
      if (!runtimeReady) {
        runtimeSelect.value = health.runtime || "core";
        runtimeMode = runtimeSelect.value;
        runtimeReady = true;
        updateButtons();
        if (goSource && dirty) scheduleAutoConvert();
      }
    } catch {
      setConnection(false, "Local server disconnected");
      setWarmup(null);
    }
  }

  function receiveTourMessage(event) {
    if (event.origin !== window.location.origin || event.source !== frame.contentWindow) return;
    if (event.data?.type === "go-tour-theme") {
      document.documentElement.dataset.theme = event.data.theme === "light" ? "light" : "dark";
      return;
    }
    if (event.data?.type !== "go-tour-source" && event.data?.type !== "go-tour-run") return;

    receiveTourSource(event.data);
    if (event.data.type === "go-tour-run" && syncRunCheckbox.checked) queueSynchronizedRun();
  }

  function receiveTourSource(message) {
    const nextSource = message.source || "";
    const nextPath = message.path || "";
    const navigation = message.reason === "navigation" || (nextPath && nextPath !== pagePath);
    const sourceChanged = nextSource !== goSource;

    goSource = nextSource;
    pagePath = nextPath;
    lessonLabel.textContent = message.title || "Generated .NET";

    if (navigation) {
      convertController?.abort();
      runController?.abort();
      forgetBuild();
      synchronizedRunPending = false;
      selectSourceView("code");
      resetNavigationState();
    }

    if (!goSource) {
      conversionStatus.textContent = "This page has no editable Go source";
      forgetBuild();
      conversionID = "";
      dirty = false;
      outputs.transpile = idleStage("transpile", "This Tour page has no code to convert.");
      outputs.build = idleStage("build", "Nothing to build.");
      outputs.run = idleStage("run", "Nothing to run.");
      selectOutput("transpile");
      updateButtons();
      return;
    }

    if (navigation) {
      conversionID = "";
      dirty = true;
      conversionStatus.textContent = "Converting page...";
      scheduleAutoConvert();
    } else if (sourceChanged) {
      forgetBuild();
      conversionID = "";
      dirty = true;
      conversionStatus.textContent = "Go source changed -- convert to refresh";
    }
    updateButtons();
  }

  function queueSynchronizedRun() {
    synchronizedRunPending = true;
    processSynchronizedRun();
  }

  async function processSynchronizedRun() {
    if (!synchronizedRunPending || synchronizedRunProcessing || converting || running || !runtimeReady) return;

    synchronizedRunProcessing = true;
    try {
      if (!goSource) {
        synchronizedRunPending = false;
        return;
      }
      if (dirty || !conversionID) {
        const converted = await convertSource(false);
        if (!converted) {
          synchronizedRunPending = false;
          return;
        }
      }
      if (!synchronizedRunPending || dirty || !conversionID) return;
      synchronizedRunPending = false;
      await runDotNet();
    } finally {
      synchronizedRunProcessing = false;
      if (synchronizedRunPending) processSynchronizedRun();
    }
  }

  function scheduleAutoConvert() {
    if (!runtimeReady) return;
    clearTimeout(autoConvertTimer);
    autoConvertTimer = setTimeout(() => {
      if (converting || running) {
        scheduleAutoConvert();
        return;
      }
      convertSource(true);
    }, 220);
  }

  async function convertSource(automatic = false) {
    if (!goSource || converting || running) return false;

    convertController?.abort();
    const controller = new AbortController();
    convertController = controller;
    const sourceAtStart = goSource;
    const pathAtStart = pagePath;
    let converted = false;

    converting = true;
    forgetBuild();
    conversionID = "";
    outputs.transpile = { id: "transpile", status: "running", output: "Transpiling Go to C#...", durationMs: 0 };
    conversionStatus.textContent = automatic ? "Converting page..." : "Converting...";
    selectOutput("transpile");
    updateButtons();

    try {
      const response = await fetch("/api/convert", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ source: sourceAtStart, name: lessonLabel.textContent, runtime: runtimeMode }),
        signal: controller.signal
      });
      const result = await response.json();
      if (!response.ok) throw new Error(result.error || `Conversion failed (${response.status})`);
      if (pagePath !== pathAtStart || goSource !== sourceAtStart) return false;

      outputs.transpile = result.stage;
      renderGeneratedFiles(result.csharp || "", result.packageInfo || "", result.project || "");
      conversionID = result.conversionId || "";
      dirty = goSource !== sourceAtStart;

      if (result.successful && !dirty) {
        conversionStatus.textContent = result.projectName ? `Converted  - ${result.projectName}` : "Converted";
        converted = true;
        if (conversionID) showBuild(requestBuild(conversionID));
        if (automatic) selectOutput("build");
      } else if (dirty) {
        conversionID = "";
        conversionStatus.textContent = "Go source changed -- convert to refresh";
      } else {
        conversionStatus.textContent = stageFailureMessage(result.stage);
      }
    } catch (error) {
      if (error.name !== "AbortError" && pagePath === pathAtStart && goSource === sourceAtStart) {
        outputs.transpile = taggedStage("transpile", "failed", outputStderr, error.message);
        conversionStatus.textContent = "Transpile failed";
      }
    } finally {
      if (convertController === controller) convertController = null;
      converting = false;
      renderOutputState();
      updateButtons();
      processSynchronizedRun();
    }
    return converted;
  }

  function buildingStage() {
    return { id: "build", status: "running", output: "Building generated .NET project...", durationMs: 0 };
  }

  // Asks for a conversion's build. The server started that build when it saved the conversion, so
  // this request is the build's result, not a second build. The request becomes buildRequest, and
  // its promise resolves to the build stage or rejects with the request's error.
  function requestBuild(id) {
    forgetBuild();
    const controller = new AbortController();
    const request = { conversionId: id, controller, settled: false, stage: null, promise: null };
    request.promise = (async () => {
      try {
        const response = await fetch("/api/build", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ conversionId: id }),
          signal: controller.signal
        });
        const stage = await response.json();
        if (!response.ok) throw new Error(stage.error || `Build failed (${response.status})`);
        request.stage = stage;
        return stage;
      } finally {
        request.settled = true;
      }
    })();
    buildRequest = request;
    return request;
  }

  // Abandons the page's build request: its conversion is no longer the current one. The server's
  // build is not stopped by this; only the page stops listening for it.
  function forgetBuild() {
    buildRequest?.controller.abort();
    buildRequest = null;
  }

  // Shows a conversion's build in the Build tab while it runs and its result when it lands, as Run
  // renders it. While Run is waiting on this same request, Run renders the result instead.
  function showBuild(request) {
    const pathAtStart = pagePath;
    const current = () => !running && buildRequest === request &&
      pagePath === pathAtStart && conversionID === request.conversionId;
    outputs.build = buildingStage();
    request.promise.then(stage => {
      if (!current()) return;
      outputs.build = stage;
      if (stage.status !== "passed") {
        conversionStatus.textContent = "Build failed";
      } else {
        // Built and waiting for Run, as the Go side waits: settle on the .NET Run tab, unless the
        // reader has moved off the Build tab the page chose for them.
        outputs.run = idleStage("run", "Built and ready: press Run to see the .NET output.");
        if (activeOutput === "build") activeOutput = "run";
      }
      renderOutputState();
    }, error => {
      if (error.name === "AbortError" || !current()) return;
      outputs.build = taggedStage("build", "failed", outputStderr, error.message);
      conversionStatus.textContent = "Build failed";
      renderOutputState();
    });
  }

  async function runDotNet() {
    if (running) {
      killRun();
      return;
    }
    if (!conversionID || dirty || converting) return;

    const controller = new AbortController();
    runController = controller;
    const conversionAtStart = conversionID;
    const pathAtStart = pagePath;
    // The conversion's own build request is reused while it is in flight or once it has passed; a
    // failed or abandoned one is asked again, and the server answers with its finished build.
    const reuse = buildRequest?.conversionId === conversionAtStart &&
      (!buildRequest.settled || buildRequest.stage?.status === "passed");
    const build = reuse ? buildRequest : requestBuild(conversionAtStart);
    // Kill stops the page waiting for the build, as it always has; the server's build carries on.
    controller.signal.addEventListener("abort", () => build.controller.abort(), { once: true });
    const built = build.stage?.status === "passed";
    running = true;
    if (!built) {
      outputs.build = buildingStage();
      outputs.run = idleStage("run", "Waiting for a successful build.");
      conversionStatus.textContent = "Building...";
      selectOutput("build");
    }
    updateButtons();

    try {
      const requestBody = JSON.stringify({ conversionId: conversionAtStart });
      const buildStage = built ? build.stage : await build.promise;
      if (pagePath !== pathAtStart || conversionID !== conversionAtStart) return;
      outputs.build = buildStage;
      renderOutputState();

      if (buildStage.status !== "passed") {
        outputs.run = idleStage("run", "Run was not started because the build failed.");
        conversionStatus.textContent = "Build failed";
        selectOutput("build");
        return;
      }

      outputs.run = { id: "run", status: "running", output: "Running generated .NET program...", durationMs: 0 };
      conversionStatus.textContent = "Running .NET...";
      selectOutput("run");

      const runResponse = await fetch("/api/execute", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: requestBody,
        signal: controller.signal
      });
      const runStage = await runResponse.json();
      if (!runResponse.ok) throw new Error(runStage.error || `.NET run failed (${runResponse.status})`);
      if (pagePath !== pathAtStart || conversionID !== conversionAtStart) return;
      outputs.run = runStage;
      conversionStatus.textContent = runStage.status === "passed" ? ".NET run completed" : ".NET run failed";
      selectOutput("run");
    } catch (error) {
      const stale = pagePath !== pathAtStart || conversionID !== conversionAtStart;
      if (stale) return;
      const active = outputs.run.status === "running" ? "run" : "build";
      if (error.name === "AbortError") {
        const killed = active === "run" ? killedProgramOutput() : "Killed.";
        outputs[active] = taggedStage(active, "killed", outputSystem, killed);
        if (active === "build") outputs.run = idleStage("run", "Run was not started.");
        conversionStatus.textContent = ".NET process killed";
      } else {
        outputs[active] = taggedStage(active, "failed", outputStderr, error.message);
        conversionStatus.textContent = active === "build" ? "Build failed" : ".NET run failed";
      }
      selectOutput(active);
    } finally {
      if (runController === controller) runController = null;
      running = false;
      renderOutputState();
      updateButtons();
      processSynchronizedRun();
    }
  }

  function killRun() {
    runController?.abort();
  }

  function killedProgramOutput() {
    const platform = navigator.userAgentData?.platform || navigator.platform || "";
    return `Program exited: ${/^Win/i.test(platform) ? "exit status 1" : "signal: killed"}`;
  }

  function updateButtons() {
    convertButton.disabled = !goSource || converting || running || !dirty;
    convertButton.textContent = converting ? "Converting..." : "Convert";

    runButton.disabled = !running && (!conversionID || dirty || converting);
    runButton.textContent = running ? "Kill" : "Run";
    runButton.classList.toggle("kill", running);
    runtimeSelect.disabled = !runtimeReady || converting || running;
  }

  function renderGeneratedFiles(csharp, packageInfo, project) {
    codeView.innerHTML = highlightCSharp(csharp || "// No C# source was emitted.");
    packageInfoView.innerHTML = highlightCSharp(packageInfo || "// No package metadata was emitted.");
    projectView.innerHTML = highlightXML(project || "<!-- No project file was emitted. -->");
  }

  function resetNavigationState() {
    codeView.textContent = "";
    packageInfoView.textContent = "";
    projectView.textContent = "";
    outputs.transpile = idleStage("transpile", "");
    outputs.build = idleStage("build", "");
    outputs.run = idleStage("run", "");
    selectOutput("transpile");
  }

  function selectSourceView(name) {
    for (const button of sourceTabs.querySelectorAll("[data-view]")) {
      const selected = button.dataset.view === name;
      button.setAttribute("aria-selected", String(selected));
      document.querySelector(`#${button.dataset.view}-view`).hidden = !selected;
    }
  }

  function selectOutput(id) {
    activeOutput = id;
    renderOutputState();
  }

  function renderOutputState() {
    for (const button of outputTabs.querySelectorAll("[data-output]")) {
      const stage = outputs[button.dataset.output];
      button.className = stage.status;
      button.setAttribute("aria-selected", String(button.dataset.output === activeOutput));
    }
    const stage = outputs[activeOutput];
    renderOutputText(stage);
    outputTiming.textContent = stage.durationMs ? `${(stage.durationMs / 1000).toFixed(2)}s` : "";
    outputScroll.scrollTop = 0;
    outputScroll.scrollLeft = 0;
  }

  // Each stream keeps its own span, so the Tour's colors carry over: what the
  // program wrote to standard error is red, and the notices this host adds are
  // muted. A stage without tagged streams is plain text, as it was before.
  function renderOutputText(stage) {
    outputView.replaceChildren();
    const segments = stage.segments?.length
      ? stage.segments
      : [{ kind: outputStdout, text: stage.output ?? "(no output)" }];
    for (const segment of segments) {
      const span = document.createElement("span");
      span.className = `output-${segment.kind}`;
      const text = segment.text ?? "";
      if (stage.id === "run" && segment.kind === outputStdout) appendProgramOutput(span, text);
      else span.textContent = text;
      outputView.append(span);
    }
  }

  // golang.org/x/tour/pic writes a picture as a single base64 PNG line prefixed
  // with IMAGE:, which the Tour renders as a picture rather than as text.
  // Everything the program prints around such a line stays ordinary output.
  // The line's own newline goes with it, as the Tour drops that whole write.
  function appendProgramOutput(target, text) {
    const imageLine = /^IMAGE:([A-Za-z0-9+/]+={0,2})[^\S\n]*\n?/gm;
    let index = 0;
    for (const match of text.matchAll(imageLine)) {
      const image = programImage(match[1]);
      if (!image) continue;
      target.append(document.createTextNode(text.slice(index, match.index)), image);
      index = match.index + match[0].length;
    }
    target.append(document.createTextNode(text.slice(index)));
  }

  function programImage(payload) {
    let decoded;
    try {
      decoded = atob(payload);
    } catch {
      // A line the program wrote itself, not a picture. Leave it readable.
      return null;
    }
    // Only a complete PNG becomes a picture, so an unrelated or partial payload
    // stays readable as text rather than rendering as a broken image.
    if (!decoded.startsWith("\x89PNG\r\n\x1a\n") || !decoded.endsWith("IEND\xae\x42\x60\x82")) return null;
    const image = document.createElement("img");
    image.src = `data:image/png;base64,${payload}`;
    image.alt = "Picture written by the program";
    return image;
  }

  function highlightCSharp(source) {
    const keywords = new Set([
      "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "class",
      "const", "continue", "decimal", "default", "delegate", "do", "double", "else", "enum",
      "event", "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach",
      "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock", "long",
      "namespace", "new", "null", "object", "operator", "out", "override", "params", "partial",
      "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short",
      "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true",
      "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "var", "virtual",
      "void", "volatile", "while"
    ]);
    // Keep CR out of line-comment tokens so highlighted CRLF remains one
    // line break when HTML normalizes text across span boundaries.
    const token = /\/\/[^\r\n]*|\/\*[\s\S]*?\*\/|@"(?:""|[^"])*"|"(?:\\.|[^"\\])*"|'(?:\\.|[^'\\])+'|\b\d+(?:\.\d+)?\b|\b[A-Za-z_@][A-Za-z0-9_@]*\b/g;
    let result = "";
    let index = 0;
    for (const match of source.matchAll(token)) {
      result += escapeHTML(source.slice(index, match.index));
      const value = match[0];
      let kind = "";
      if (value.startsWith("//") || value.startsWith("/*")) kind = "comment";
      else if (value.startsWith('"') || value.startsWith('@"') || value.startsWith("'")) kind = "string";
      else if (/^\d/.test(value)) kind = "number";
      else if (keywords.has(value)) kind = "keyword";
      else if (/^[A-Z]/.test(value)) kind = "type";
      result += kind ? `<span class="tok-${kind}">${escapeHTML(value)}</span>` : escapeHTML(value);
      index = match.index + value.length;
    }
    return result + escapeHTML(source.slice(index));
  }

  function highlightXML(source) {
    const token = /<!--[\s\S]*?-->|<\/?[A-Za-z][^>]*>/g;
    let result = "";
    let index = 0;
    for (const match of source.matchAll(token)) {
      result += escapeHTML(source.slice(index, match.index));
      const kind = match[0].startsWith("<!--") ? "comment" : "xml";
      result += `<span class="tok-${kind}">${escapeHTML(match[0])}</span>`;
      index = match.index + match[0].length;
    }
    return result + escapeHTML(source.slice(index));
  }

  function escapeHTML(value) {
    return value.replace(/[&<>"']/g, character => ({
      "&": "&amp;",
      "<": "&lt;",
      ">": "&gt;",
      '"': "&quot;",
      "'": "&#39;"
    })[character]);
  }

  function setDivider(percent) {
    const bounds = workspace.getBoundingClientRect();
    if (bounds.width < 900) return;
    const minLeft = Math.min(480, bounds.width * .45);
    const minRight = 300;
    const pixels = Math.max(minLeft, Math.min(bounds.width - minRight - 8, bounds.width * percent / 100));
    const actual = pixels / bounds.width * 100;
    workspace.style.setProperty("--left-width", `${pixels}px`);
    divider.setAttribute("aria-valuenow", String(Math.round(actual)));
  }

  divider.addEventListener("pointerdown", event => {
    divider.setPointerCapture(event.pointerId);
    divider.classList.add("dragging");
  });
  divider.addEventListener("pointermove", event => {
    if (!divider.hasPointerCapture(event.pointerId)) return;
    const bounds = workspace.getBoundingClientRect();
    setDivider((event.clientX - bounds.left) / bounds.width * 100);
  });
  divider.addEventListener("pointerup", event => {
    divider.releasePointerCapture(event.pointerId);
    divider.classList.remove("dragging");
  });
  divider.addEventListener("keydown", event => {
    if (event.key !== "ArrowLeft" && event.key !== "ArrowRight") return;
    event.preventDefault();
    const current = Number(divider.getAttribute("aria-valuenow")) || 67;
    setDivider(current + (event.key === "ArrowRight" ? 2 : -2));
  });

  window.addEventListener("message", receiveTourMessage);
  frame.addEventListener("load", () => {
    frame.contentWindow?.postMessage({ type: "go2cs-request-source" }, window.location.origin);
  });
  sourceTabs.addEventListener("click", event => {
    const button = event.target.closest("[data-view]");
    if (button) selectSourceView(button.dataset.view);
  });
  outputTabs.addEventListener("click", event => {
    const button = event.target.closest("[data-output]");
    if (button) selectOutput(button.dataset.output);
  });
  runtimeSelect.addEventListener("change", () => {
    runtimeMode = runtimeSelect.value;
    forgetBuild();
    conversionID = "";
    dirty = Boolean(goSource);
    outputs.build = idleStage("build", "Run the converted project to see build output.");
    outputs.run = idleStage("run", "Run the converted project to see .NET output.");
    conversionStatus.textContent = goSource ? "Switching runtime..." : "Waiting for Go source";
    updateButtons();
    if (goSource) scheduleAutoConvert();
  });
  syncRunCheckbox.addEventListener("change", () => {
    if (!syncRunCheckbox.checked) synchronizedRunPending = false;
    try {
      localStorage.setItem(syncRunStorageKey, String(syncRunCheckbox.checked));
    } catch {
      // Keep the current-page preference when storage is unavailable.
    }
  });
  convertButton.addEventListener("click", () => convertSource(false));
  runButton.addEventListener("click", runDotNet);
  window.addEventListener("keydown", event => {
    if ((event.ctrlKey || event.metaKey) && event.key === "Enter") {
      event.preventDefault();
      convertSource(false);
    } else if (event.shiftKey && event.key === "Enter") {
      event.preventDefault();
      runDotNet();
    }
  });
  window.addEventListener("resize", () => setDivider(Number(divider.getAttribute("aria-valuenow")) || 67));

  renderOutputState();
  updateButtons();
  checkHealth();
  setInterval(checkHealth, 15000);
})();
