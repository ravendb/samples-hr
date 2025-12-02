import React, { useState } from 'react';
import { ChatInterface } from './components/ChatInterface';
import { UsageCharts, RateLimitAlert } from './components/UsageCharts';

function App() {
  const [rateLimitAlert, setRateLimitAlert] = useState<RateLimitAlert | null>(null);

  return (
    <div className="app-container">
      <ChatInterface onRateLimitError={setRateLimitAlert} />
      <div className="usage-sidebar">
        <UsageCharts
          rateLimitAlert={rateLimitAlert}
          onAlertDismiss={() => setRateLimitAlert(null)}
        />
      </div>
    </div>
  );
}

export default App;