import { InputOTP, InputOTPGroup, InputOTPSlot } from "./ui/input-otp";

interface PhoneInputProps {
  value: string;
  onChange: (value: string) => void;
  disabled?: boolean;
}

function handleNumericChange(
  onChange: (value: string) => void
): (value: string) => void {
  return function (nextValue: string) {
    const numericValue = nextValue.replace(/\D/g, "");
    onChange(numericValue);
  };
}

export function PhoneInput({ value, onChange, disabled }: PhoneInputProps) {
  return (
    <div className="relative flex justify-center items-center">
      <InputOTP
        maxLength={10}
        value={value}
        onChange={handleNumericChange(onChange)}
        disabled={disabled}
        inputMode="numeric"
        pattern="[0-9]*"
        autoComplete="tel"
      >
        <InputOTPGroup>
          <InputOTPSlot index={0} />
          <InputOTPSlot index={1} />
          <InputOTPSlot index={2} />
        </InputOTPGroup>
        <span className="text-muted-foreground select-none"> </span>
        <InputOTPGroup>
          <InputOTPSlot index={3} />
          <InputOTPSlot index={4} />
          <InputOTPSlot index={5} />
        </InputOTPGroup>
        <span className="text-muted-foreground select-none"> </span>
        <InputOTPGroup>
          <InputOTPSlot index={6} />
          <InputOTPSlot index={7} />
          <InputOTPSlot index={8} />
          <InputOTPSlot index={9} />
        </InputOTPGroup>
      </InputOTP>
    </div>
  );
}
