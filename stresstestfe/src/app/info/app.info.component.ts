import {Component} from '@angular/core';
import {AccordionModule} from 'primeng/accordion';

@Component({
  selector: 'app-rate-limiting-info',
  template: `
    <div class="p-6 rounded-2xl shadow-md bg-white max-w-3xl mx-auto mt-8">
      <h2 class="text-2xl font-bold mb-4 text-blue-800">Rate Limiting Overview</h2>
      <p-accordion value="0">
        <p-accordion-panel value="0">
          <p-accordion-header>Specific Info</p-accordion-header>
          <p-accordion-content>
            <div class="mb-6">
              <h3 class="text-xl font-semibold text-gray-700">1. HTTP Client Configuration</h3>
              <ul class="list-disc list-inside text-gray-600 mt-2">
                <li>Applies to: <strong>RoomClient, BookClient, LoanClient, UserClient</strong></li>
                <li>Request timeout: <strong>5 seconds</strong></li>
                <li>Bulkhead isolation: <strong>20 concurrent executions</strong> + <strong>50 queued</strong></li>
              </ul>
            </div>

            <div class="mb-6">
              <h3 class="text-xl font-semibold text-gray-700">2. Retry Policy</h3>
              <ul class="list-disc list-inside text-gray-600 mt-2">
                <li><strong>5 retry attempts</strong> with exponential backoff</li>
                <li>Maximum delay between retries: <strong>2.5 seconds</strong></li>
                <li>Triggers on transient failures (e.g., network issues, <strong>5xx</strong> responses)</li>
              </ul>
            </div>

            <div class="mb-6">
              <h3 class="text-xl font-semibold text-gray-700">3. Circuit Breaker</h3>
              <ul class="list-disc list-inside text-gray-600 mt-2">
                <li>Trips when <strong>5%</strong> of requests fail</li>
                <li>Requires a minimum of <strong>75 requests</strong> (MinimumThroughput)</li>
                <li>Monitors over a <strong>35-second</strong> window (SamplingDuration)</li>
                <li>Stays open for <strong>6 seconds</strong> after tripping (BreakDuration)</li>
              </ul>
            </div>

            <div class="mb-6">
              <h3 class="text-xl font-semibold text-gray-700">4. Rate Limiting</h3>
              <ul class="list-disc list-inside text-gray-600 mt-2">
                <li><strong>YLimiter (Global):</strong>
                  <ul class="list-disc list-inside ml-4 mt-1 text-gray-600">
                    <li><strong>800 requests per minute</strong> across all users</li>
                    <li><strong>225 queued requests</strong> when limit is reached</li>
                    <li>FIFO processing order</li>
                  </ul>
                </li>
                <li class="mt-2"><strong>XLimiter (Per-IP):</strong>
                  <ul class="list-disc list-inside ml-4 mt-1 text-gray-600">
                    <li><strong>400 requests per minute</strong> per IP address</li>
                    <li><strong>100 queued requests</strong> per IP</li>
                    <li>Automatic permit replenishment</li>
                  </ul>
                </li>
                <li class="mt-2">Both limiters <span class="text-red-600 font-semibold">log rejected requests</span>
                  with IP and path info
                </li>
              </ul>
            </div>
          </p-accordion-content>
        </p-accordion-panel>
      </p-accordion>
    </div>
  `,
  standalone: true,
  imports: [AccordionModule],
  styles: []
})
export class RateLimitingInfoComponent {
}
