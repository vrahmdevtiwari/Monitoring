class PatchAutoRefresher {
    constructor(intervalMs = 5 * 60 * 1000) {
        this.intervalMs = intervalMs;
        this.timerId = null;
        this.retryCount = 0;
        this.maxRetries = 3;
    }

    start() {
        // Load immediately first
        this._loadWithRetry();

        // Then schedule recurring loads
        this._scheduleNext();

        // Pause when tab is hidden, resume when visible
        document.addEventListener('visibilitychange', () => {
            if (document.hidden) {
                this._pause();
            } else {
                this._resume();
            }
        });

        // Cleanup on page unload
        window.addEventListener('beforeunload', () => this.stop());

        console.log(`🔄 Auto-refresh started (every ${this.intervalMs / 1000 / 60} mins)`);
    }

    stop() {
        if (this.timerId) {
            clearInterval(this.timerId);
            this.timerId = null;
            console.log('⏹️ Auto-refresh stopped');
        }
    }

    _scheduleNext() {
        this.timerId = setInterval(() => {
            this._loadWithRetry();
        }, this.intervalMs);
    }

    _pause() {
        if (this.timerId) {
            clearInterval(this.timerId);
            this.timerId = null;
            console.log('⏸️ Auto-refresh paused (tab hidden)');
        }
    }

    _resume() {
        if (!this.timerId) {
            this._scheduleNext();
            console.log('▶️ Auto-refresh resumed (tab visible)');
        }
    }

    async _loadWithRetry() {
        try {
            await this._doLoad();
            this.retryCount = 0; // Reset on success
        } catch (err) {
            this.retryCount++;
            console.warn(`⚠️ Load attempt ${this.retryCount} failed:`, err);

            if (this.retryCount < this.maxRetries) {
                // Exponential backoff: 1s, 2s, 4s...
                const delay = Math.min(1000 * Math.pow(2, this.retryCount), 30000);
                console.log(`🔁 Retrying in ${delay / 1000}s...`);
                setTimeout(() => this._loadWithRetry(), delay);
            } else {
                console.error('❌ Max retries reached. Giving up.');
                this.retryCount = 0;
            }
        }
    }

    async _doLoad() {
        const res = await fetch('/CISRepo/SyncNow', {
            method: 'GET',
            // Add auth headers if needed:
            // headers: { 'Authorization': `Bearer ${token}` }
        });

        if (!res.ok) {
            throw new Error(`HTTP ${res.status}: ${res.statusText}`);
        }
                
        console.log('✅ Patches refreshed at', new Date().toLocaleTimeString());
    }
}

// ============ USAGE ============
const refresher = new PatchAutoRefresher(); // Default: 4 hours
refresher.start();

// To manually stop later:
// refresher.stop();