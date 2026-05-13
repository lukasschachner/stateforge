const endpoints = {
    catalog: '/api/order-workflow/events/catalog',
    state: '/api/order-workflow/runtime/state',
    graph: '/api/order-workflow/definition/graph',
    preview: '/api/order-workflow/runtime/events/preview',
    apply: '/api/order-workflow/runtime/events/apply',
    reset: '/api/order-workflow/runtime/reset'
};

const state = {
    catalog: [],
    runtime: null,
    graph: null,
    lastPreview: null,
    lastApply: null,
    busy: false,
    mermaidReady: false,
    diagramRenderToken: 0
};

const elements = {
    statusBanner: document.getElementById('status-banner'),
    runtimeSummary: document.getElementById('runtime-summary'),
    paymentSummary: document.getElementById('payment-summary'),
    paymentProgressBar: document.getElementById('payment-progress-bar'),
    permittedEvents: document.getElementById('permitted-events'),
    eventType: document.getElementById('event-type'),
    eventDescription: document.getElementById('event-description'),
    eventPermitted: document.getElementById('event-permitted'),
    eventFields: document.getElementById('event-fields'),
    previewSummary: document.getElementById('preview-summary'),
    applySummary: document.getElementById('apply-summary'),
    overlaySummary: document.getElementById('overlay-summary'),
    diagramContainer: document.getElementById('diagram-container'),
    nodeList: document.getElementById('node-list'),
    edgeList: document.getElementById('edge-list'),
    previewButton: document.getElementById('preview-button'),
    applyButton: document.getElementById('apply-button'),
    resetButton: document.getElementById('reset-button')
};

initialize().catch(error => {
    setStatus(`Initialization failed: ${error.message}`, 'error');
    elements.runtimeSummary.textContent = 'Initialization failed.';
    elements.diagramContainer.textContent = 'Diagram unavailable.';
});

async function initialize() {
    initializeMermaid();
    wireActions();
    await loadCatalog();
    await refreshRuntimeAndGraph();
}

function initializeMermaid() {
    if (!window.mermaid) {
        state.mermaidReady = false;
        elements.diagramContainer.textContent =
            'Mermaid library was not loaded. Graph diagram rendering is unavailable.';
        return;
    }

    window.mermaid.initialize({
        startOnLoad: false,
        securityLevel: 'loose',
        theme: 'default'
    });

    state.mermaidReady = true;
}

function wireActions() {
    elements.eventType.addEventListener('change', () => {
        renderSelectedEventForm();
        renderEventPermittedHint();
    });

    elements.previewButton.addEventListener('click', () =>
        executeAction('Previewing event...', async () => {
            const request = buildEventRequest();
            const response = await postJson(endpoints.preview, request);
            renderPreviewResponse(response);
            await refreshRuntimeAndGraph();
            setStatus('Preview completed.', 'success');
        }));

    elements.applyButton.addEventListener('click', () =>
        executeAction('Applying event...', async () => {
            const request = buildEventRequest();
            const response = await postJson(endpoints.apply, request);
            renderApplyResponse(response);
            await refreshRuntimeAndGraph();
            setStatus(`Apply completed: ${response.category} (${response.resultingState}).`,
                response.committed ? 'success' : 'error');
        }));

    elements.resetButton.addEventListener('click', () =>
        executeAction('Resetting runtime...', async () => {
            await postJson(endpoints.reset, {});
            state.lastPreview = null;
            state.lastApply = null;
            elements.previewSummary.textContent = 'No preview yet.';
            elements.applySummary.textContent = 'No event applied yet.';
            await refreshRuntimeAndGraph();
            setStatus('Runtime reset completed.', 'success');
        }));
}

async function executeAction(startMessage, action) {
    if (state.busy) return;

    setBusy(true);
    setStatus(startMessage, 'info');

    try {
        await action();
    } catch (error) {
        setStatus(error.message, 'error');
    } finally {
        setBusy(false);
    }
}

function setBusy(isBusy) {
    state.busy = isBusy;
    elements.previewButton.disabled = isBusy;
    elements.applyButton.disabled = isBusy;
    elements.resetButton.disabled = isBusy;
    elements.eventType.disabled = isBusy;
}

function setStatus(message, kind) {
    elements.statusBanner.textContent = message;
    elements.statusBanner.className = `status ${kind}`;
}

async function loadCatalog() {
    const response = await fetchJson(endpoints.catalog);
    state.catalog = response.events || [];

    elements.eventType.innerHTML = '';
    for (const descriptor of state.catalog) {
        const option = document.createElement('option');
        option.value = descriptor.eventType;
        option.textContent = descriptor.eventType;
        elements.eventType.append(option);
    }

    if (state.catalog.length > 0) {
        elements.eventType.value = state.catalog[0].eventType;
    }

    renderSelectedEventForm();
}

async function refreshRuntimeAndGraph() {
    const [runtimeResponse, graphResponse] = await Promise.all([
        fetchJson(endpoints.state),
        fetchJson(endpoints.graph)
    ]);

    state.runtime = runtimeResponse;
    state.graph = graphResponse;

    renderRuntimeState();
    renderEventPermittedHint();
    renderGraph();
    await renderMermaidDiagram();
}

function renderRuntimeState() {
    const runtime = state.runtime;
    if (!runtime) {
        elements.runtimeSummary.textContent = 'Runtime unavailable.';
        return;
    }

    const shape = runtime.activeShape;
    const lines = [
        `Current state: ${runtime.currentState}`,
        `Complete: ${runtime.isComplete}`,
        `Shape: ${shape.kind} (sequence=${shape.sequence})`
    ];

    if (shape.owningCompositeState) {
        lines.push(`Owning composite: ${shape.owningCompositeState}`);
    }

    if (shape.activePath && shape.activePath.length > 0) {
        lines.push(`Active path: ${shape.activePath.join(' -> ')}`);
    }

    if (shape.regions && shape.regions.length > 0) {
        for (const region of shape.regions) {
            lines.push(
                `Region ${region.regionName}: ${region.activeLeafState} terminal=${region.isTerminal} complete=${region.isComplete}`);
        }
    }

    elements.runtimeSummary.textContent = lines.join('\n');

    renderPaymentProgress(runtime.paymentProgress);

    elements.permittedEvents.innerHTML = '';
    const permitted = runtime.permittedEvents || [];
    if (permitted.length === 0) {
        const chip = document.createElement('span');
        chip.className = 'chip muted';
        chip.textContent = 'No permitted events';
        elements.permittedEvents.append(chip);
    } else {
        for (const eventType of permitted) {
            const chip = document.createElement('span');
            chip.className = 'chip';
            chip.textContent = eventType;
            elements.permittedEvents.append(chip);
        }
    }
}

function renderPaymentProgress(paymentProgress) {
    if (!paymentProgress) {
        elements.paymentSummary.textContent = 'Payment progress unavailable.';
        elements.paymentProgressBar.style.width = '0%';
        elements.paymentProgressBar.className = 'progress-bar';
        return;
    }

    const lines = [
        `Required: ${paymentProgress.requiredAmount}`,
        `Captured: ${paymentProgress.capturedAmount}`,
        `Remaining: ${paymentProgress.remainingAmount}`,
        `Progress: ${paymentProgress.progressPercent}%`,
        `Complete: ${paymentProgress.isPaymentComplete}`
    ];
    elements.paymentSummary.textContent = lines.join('\n');

    const width = Math.max(0, Math.min(100, Number(paymentProgress.progressPercent)));
    elements.paymentProgressBar.style.width = `${width}%`;
    elements.paymentProgressBar.className = paymentProgress.isPaymentComplete
        ? 'progress-bar complete'
        : 'progress-bar';
}

function renderEventPermittedHint() {
    const descriptor = getSelectedDescriptor();
    if (!descriptor || !state.runtime) {
        elements.eventPermitted.textContent = '';
        return;
    }

    const isPermitted = (state.runtime.permittedEvents || []).includes(descriptor.eventType);
    elements.eventPermitted.textContent = isPermitted
        ? 'Selected event is currently permitted.'
        : 'Selected event is currently not permitted (preview/apply shows diagnostics).';
    elements.eventPermitted.className = `hint ${isPermitted ? 'success' : 'warn'}`;
}

function renderGraph() {
    const graph = state.graph;
    if (!graph) {
        elements.overlaySummary.textContent = 'Graph unavailable.';
        return;
    }

    const overlay = graph.overlay;
    if (!overlay) {
        elements.overlaySummary.textContent = 'Overlay not available.';
    } else {
        const lines = [
            `Overlay shape: ${overlay.shapeKind}`,
            `Overlay sequence: ${overlay.sequence}`,
            `Overlay complete: ${overlay.isComplete}`
        ];

        for (const region of overlay.regions || []) {
            lines.push(`Region ${region.regionName}: activeNode=${region.activeLeafNodeId}, terminal=${region.isTerminal}`);
        }

        elements.overlaySummary.textContent = lines.join('\n');
    }

    const highlight = getTransitionHighlightState();

    elements.nodeList.innerHTML = '';
    for (const node of graph.nodes || []) {
        const item = document.createElement('li');
        item.className = 'node-item';
        if (node.isActive) item.classList.add('active');
        if (node.isInActivePath) item.classList.add('path');

        item.textContent = `${node.state} (${node.id})`
            + ` terminal=${node.isTerminal}`
            + ` composite=${node.isComposite}`;
        elements.nodeList.append(item);
    }

    elements.edgeList.innerHTML = '';
    for (const edge of graph.edges || []) {
        const item = document.createElement('li');
        item.className = 'edge-item';

        const key = transitionKey(edge);
        if (highlight.previewKeys.has(key)) item.classList.add('preview');
        if (highlight.applyKeys.has(key)) item.classList.add('applied');
        if (highlight.candidateIds.has(edge.id)) item.classList.add('candidate');

        item.textContent = `${edge.id}: ${edge.sourceState} --${edge.eventDisplayName}/${edge.triggerKind}--> ${edge.targetState}`;
        elements.edgeList.append(item);
    }
}

async function renderMermaidDiagram() {
    if (!state.mermaidReady) return;
    if (!state.graph) {
        elements.diagramContainer.textContent = 'Diagram unavailable.';
        return;
    }

    const highlight = getTransitionHighlightState();
    const source = buildMermaidSource(state.graph, highlight);

    const token = ++state.diagramRenderToken;
    const renderId = `order-workflow-${token}`;

    try {
        const { svg, bindFunctions } = await window.mermaid.render(renderId, source);
        if (token !== state.diagramRenderToken) return;

        elements.diagramContainer.innerHTML = svg;
        if (typeof bindFunctions === 'function') {
            bindFunctions(elements.diagramContainer);
        }
    } catch (error) {
        if (token !== state.diagramRenderToken) return;
        elements.diagramContainer.textContent = `Diagram rendering failed: ${error.message}`;
    }
}

function buildMermaidSource(graph, highlight) {
    const lines = ['flowchart LR'];
    const nodeAliases = new Map();

    lines.push('  classDef terminal fill:#e8f5e9,stroke:#2e7d32,stroke-width:2px;');
    lines.push('  classDef runtimePath stroke:#f9a825,stroke-width:2px;');
    lines.push('  classDef runtimeActive fill:#fff8e1,stroke:#f57f17,stroke-width:3px;');

    const nodes = graph.nodes || [];
    for (let i = 0; i < nodes.length; i++) {
        const node = nodes[i];
        const alias = `n${i}`;
        nodeAliases.set(node.id, alias);
        lines.push(`  ${alias}["${escapeMermaidLabel(node.state)}"]`);
    }

    const edges = graph.edges || [];
    for (let i = 0; i < edges.length; i++) {
        const edge = edges[i];
        const source = nodeAliases.get(edge.sourceNodeId);
        const target = nodeAliases.get(edge.targetNodeId);
        if (!source || !target) continue;

        const edgeLabel = `${edge.eventDisplayName}/${edge.triggerKind}`;
        lines.push(`  ${source} -- "${escapeMermaidLabel(edgeLabel)}" --> ${target}`);
    }

    for (const node of nodes) {
        const alias = nodeAliases.get(node.id);
        if (!alias) continue;

        if (node.isTerminal) lines.push(`  class ${alias} terminal`);
        if (node.isInActivePath) lines.push(`  class ${alias} runtimePath`);
        if (node.isActive) lines.push(`  class ${alias} runtimeActive`);
    }

    for (let i = 0; i < edges.length; i++) {
        const edge = edges[i];
        const key = transitionKey(edge);
        const styles = [];

        if (highlight.previewKeys.has(key)) {
            styles.push('stroke:#0ea5e9', 'stroke-width:3px');
        }

        if (highlight.applyKeys.has(key)) {
            styles.push('stroke:#10b981', 'stroke-width:3px');
        }

        if (highlight.candidateIds.has(edge.id)) {
            styles.push('stroke-dasharray: 6 3');
        }

        if (styles.length > 0) {
            lines.push(`  linkStyle ${i} ${styles.join(',')}`);
        }
    }

    return lines.join('\n');
}

function escapeMermaidLabel(value) {
    return String(value ?? '')
        .replace(/\\/g, '\\\\')
        .replace(/"/g, '\\"')
        .replace(/\n/g, ' ');
}

function getTransitionHighlightState() {
    return {
        previewKeys: new Set((state.lastPreview?.selectedTransitions || []).map(transitionKey)),
        applyKeys: new Set((state.lastApply?.transitions || []).map(transitionKey)),
        candidateIds: new Set(state.lastPreview?.candidateTransitionIds || [])
    };
}

function renderSelectedEventForm() {
    const descriptor = getSelectedDescriptor();
    if (!descriptor) {
        elements.eventDescription.textContent = 'No events available.';
        elements.eventFields.innerHTML = '';
        return;
    }

    elements.eventDescription.textContent = descriptor.description;
    elements.eventDescription.className = 'hint';
    elements.eventFields.innerHTML = '';

    for (const field of descriptor.fields || []) {
        const wrapper = document.createElement('div');
        wrapper.className = 'field';

        const label = document.createElement('label');
        label.textContent = `${field.name} (${field.type})`;
        label.htmlFor = `field-${field.name}`;

        const input = document.createElement('input');
        input.id = `field-${field.name}`;
        input.dataset.fieldName = field.name;
        input.dataset.fieldType = field.type;
        input.required = field.required;
        input.placeholder = field.hint || '';
        input.type = field.type === 'int' || field.type === 'decimal' ? 'number' : 'text';

        const exampleValue = descriptor.examplePayload ? descriptor.examplePayload[field.name] : null;
        if (exampleValue !== undefined && exampleValue !== null) {
            input.value = `${exampleValue}`;
        }

        wrapper.append(label, input);
        elements.eventFields.append(wrapper);
    }
}

function buildEventRequest() {
    const descriptor = getSelectedDescriptor();
    if (!descriptor) {
        throw new Error('No event descriptor is selected.');
    }

    const payload = {};
    for (const field of descriptor.fields || []) {
        const input = document.getElementById(`field-${field.name}`);
        const rawValue = input?.value?.trim() ?? '';

        if (field.required && rawValue.length === 0) {
            throw new Error(`Field '${field.name}' is required.`);
        }

        if (field.type === 'int') {
            const parsed = Number.parseInt(rawValue, 10);
            if (!Number.isInteger(parsed)) {
                throw new Error(`Field '${field.name}' must be an integer.`);
            }
            payload[field.name] = parsed;
        } else if (field.type === 'decimal') {
            const parsed = Number.parseFloat(rawValue);
            if (!Number.isFinite(parsed)) {
                throw new Error(`Field '${field.name}' must be a decimal number.`);
            }
            payload[field.name] = parsed;
        } else {
            payload[field.name] = rawValue;
        }
    }

    return {
        eventType: descriptor.eventType,
        payload
    };
}

function renderPreviewResponse(response) {
    state.lastPreview = response;

    const lines = [
        `Status: ${response.status}`,
        `Permitted: ${response.isPermitted}`,
        `Prediction: ${response.predictionCompleteness}`
    ];

    if (response.expectedTargetState) {
        lines.push(`Expected target: ${response.expectedTargetState}`);
    }

    if (response.selectedTransitions?.length > 0) {
        lines.push('Selected transitions:');
        for (const transition of response.selectedTransitions) {
            lines.push(`- ${transition.sourceState} --${transition.eventDisplayName}/${transition.triggerKind}--> ${transition.targetState}`);
        }
    }

    if (response.candidateTransitionIds?.length > 0) {
        lines.push(`Candidate IDs: ${response.candidateTransitionIds.join(', ')}`);
    }

    if (response.denialReason || response.denialMessage) {
        lines.push(`Denial: ${response.denialReason ?? 'n/a'} - ${response.denialMessage ?? ''}`);
    }

    if (response.guardDiagnostics?.length > 0) {
        lines.push('Guard diagnostics:');
        for (const guard of response.guardDiagnostics) {
            lines.push(`- #${guard.guardIndex} ${guard.displayName}: ${guard.status}${guard.message ? ` (${guard.message})` : ''}`);
        }
    }

    elements.previewSummary.textContent = lines.join('\n');
}

function renderApplyResponse(response) {
    state.lastApply = response;

    const lines = [
        `Category: ${response.category}`,
        `Committed: ${response.committed}`,
        `State change: ${response.previousState} -> ${response.resultingState}`,
        `Summary: ${response.summary}`
    ];

    if (response.paymentProgress) {
        lines.push(
            `Payment progress: ${response.paymentProgress.capturedAmount}/${response.paymentProgress.requiredAmount} (${response.paymentProgress.progressPercent}%)`);
    }

    if (response.transitions?.length > 0) {
        lines.push('Transitions:');
        for (const transition of response.transitions) {
            lines.push(`- ${transition.sourceState} --${transition.eventDisplayName}/${transition.triggerKind}--> ${transition.targetState}`);
        }
    }

    if (response.denialDiagnostics?.length > 0) {
        lines.push('Denial diagnostics:');
        for (const denial of response.denialDiagnostics) {
            lines.push(`- ${denial.reason}: ${denial.message}`);
        }
    }

    elements.applySummary.textContent = lines.join('\n');
}

function transitionKey(transition) {
    return `${transition.sourceState}|${transition.targetState}|${transition.eventDisplayName}|${transition.triggerKind}`;
}

function getSelectedDescriptor() {
    return state.catalog.find(item => item.eventType === elements.eventType.value);
}

async function fetchJson(url) {
    const response = await fetch(url);
    if (!response.ok) {
        const body = await response.text();
        throw new Error(`${url} failed (${response.status}): ${body}`);
    }

    return await response.json();
}

async function postJson(url, body) {
    const response = await fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body)
    });

    const contentType = response.headers.get('content-type') || '';
    const payload = contentType.includes('application/json')
        ? await response.json()
        : { error: await response.text() };

    if (!response.ok) {
        throw new Error(payload.error || `${url} failed (${response.status}).`);
    }

    return payload;
}
