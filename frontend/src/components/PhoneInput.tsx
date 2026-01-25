import { ClipboardEvent } from "react";
import { InputOTP, InputOTPGroup, InputOTPSlot } from "./ui/input-otp";

interface PhoneInputProps {
  value: string;
  onChange: (value: string) => void;
  disabled?: boolean;
}

// Display value is the 8 digits after "06" prefix
// Internal value (for backend) is full 10 digits starting with "06"
function getDisplayValue(internalValue: string): string {
  // If value starts with "06", strip it for display
  if (internalValue.startsWith("06")) {
    return internalValue.slice(2);
  }
  return internalValue;
}

function parsePhoneNumber(input: string): string {
  // Strip all non-digits
  let numericValue = input.replace(/\D/g, "");

  // Handle international format: +355 6X XXX XXXX -> 06X XXX XXXX
  if (numericValue.startsWith("355")) {
    numericValue = "0" + numericValue.slice(3);
  }
  // Handle pasted domestic number starting with 0 (e.g., 0673374279)
  // User won't type 0 since UI shows "+355 6", so 0 means pasted
  else if (numericValue.startsWith("0")) {
    // Already in domestic format, keep as-is
  }
  // Handle user input (8 or fewer digits without prefix) -> prepend "06"
  else if (numericValue.length <= 8) {
    numericValue = "06" + numericValue;
  }
  // Handle 9 digits starting with 6 (domestic without leading 0)
  else if (numericValue.length === 9 && numericValue.startsWith("6")) {
    numericValue = "0" + numericValue;
  }

  // Limit to 10 digits (06 + 8 digits)
  return numericValue.slice(0, 10);
}

function handlePhoneChange(
  onChange: (value: string) => void
): (value: string) => void {
  return function (nextValue: string) {
    onChange(parsePhoneNumber(nextValue));
  };
}

export function PhoneInput({ value, onChange, disabled }: PhoneInputProps) {
  const displayValue = getDisplayValue(value);

  // Intercept paste to handle full phone numbers before InputOTP truncates them
  const handlePaste = (e: ClipboardEvent<HTMLDivElement>) => {
    const pasted = e.clipboardData.getData("text");
    if (pasted) {
      onChange(parsePhoneNumber(pasted));
      e.preventDefault();
    }
  };

  return (
    <div className="relative flex justify-center items-center" onPaste={handlePaste}>
      <span className="text-text-primary font-medium mr-2 select-none">+355 6</span>
      <InputOTP
        maxLength={8}
        value={displayValue}
        onChange={handlePhoneChange(onChange)}
        disabled={disabled}
        inputMode="numeric"
        pattern="[0-9]*"
        autoComplete="tel"
      >
        <InputOTPGroup>
          <InputOTPSlot index={0} />
        </InputOTPGroup>
        <span className="text-muted-foreground select-none"> </span>
        <InputOTPGroup>
          <InputOTPSlot index={1} />
          <InputOTPSlot index={2} />
          <InputOTPSlot index={3} />
        </InputOTPGroup>
        <span className="text-muted-foreground select-none"> </span>
        <InputOTPGroup>
          <InputOTPSlot index={4} />
          <InputOTPSlot index={5} />
          <InputOTPSlot index={6} />
          <InputOTPSlot index={7} />
        </InputOTPGroup>
      </InputOTP>
    </div>
  );
}
