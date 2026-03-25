import React, { useState, useEffect } from 'react';
import { hrApi, SampleBill } from '../api';
import './BillSelectorDialog.css';

interface BillSelectorDialogProps {
  isOpen: boolean;
  onSelect: (bill: SampleBill) => void;
  onCancel: () => void;
}

export const BillSelectorDialog: React.FC<BillSelectorDialogProps> = ({
  isOpen,
  onSelect,
  onCancel,
}) => {
  const [bills, setBills] = useState<SampleBill[]>([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (isOpen && bills.length === 0) {
      setLoading(true);
      hrApi
        .getSampleBills()
        .then(setBills)
        .catch((err) => console.error('Failed to load sample bills:', err))
        .finally(() => setLoading(false));
    }
  }, [isOpen, bills.length]);

  if (!isOpen) return null;

  return (
    <div className="bill-dialog-overlay" onClick={onCancel}>
      <div
        className="bill-dialog-container"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="bill-dialog-header">
          <h2 className="bill-dialog-title">📎 Select a Bill to Report</h2>
          <p className="bill-dialog-subtitle">
            Choose a receipt from your business trip to attach to the
            conversation
          </p>
        </div>

        <div className="bill-dialog-body">
          {loading ? (
            <p>Loading bills...</p>
          ) : (
            <div className="bill-grid">
              {bills.map((bill) => (
                <div
                  key={bill.id}
                  className="bill-card"
                  onClick={() => onSelect(bill)}
                >
                  <img
                    className="bill-card-thumbnail"
                    src={hrApi.getBillImageUrl(bill.id)}
                    alt={`${bill.vendor} receipt`}
                  />
                  <div className="bill-card-info">
                    <div className="bill-card-vendor">{bill.vendor}</div>
                    <span className="bill-card-category">{bill.category}</span>
                    <div className="bill-card-description">
                      {bill.description}
                    </div>
                    <div className="bill-card-meta">
                      <span className="bill-card-date">{bill.date}</span>
                      <span className="bill-card-amount">
                        ${bill.amount.toFixed(2)}
                      </span>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        <div className="bill-dialog-footer">
          <button className="bill-cancel-button" onClick={onCancel}>
            Cancel
          </button>
        </div>
      </div>
    </div>
  );
};
