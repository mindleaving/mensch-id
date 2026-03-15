import '../styles/multistep.css';

import { useEffect, useMemo, useState } from "react";
import { Update } from "../types/frontendTypes";
import { PreviousNextButtons } from "./PreviousNextButtons";
import { resolveText } from "../helpers/Globalizer";
import { FormStep } from '../types/frontendTypes';

interface MultiStepFormProps<T> {
    steps: FormStep<T>[];
    formData: T;
    onChange: (update: Update<T>) => void;
    onSubmit: (formData: T) => Promise<void>;
    submitButtonText?: string;
}

export const MultiStepForm = <T,>(props: MultiStepFormProps<T>) => {

    const { steps, formData, onChange, onSubmit, submitButtonText } = props;

    const [ currentStep, setCurrentStep ] = useState<FormStep<T>>(steps[0]);

    useEffect(() => {
        setCurrentStep(steps[0]);
    }, [ steps ]);

    const currentStepIndex = useMemo(() => steps.indexOf(currentStep), [ currentStep, steps ]);
    const previousStep = useMemo(() => currentStepIndex > 0 ? steps[currentStepIndex-1] : undefined, [ currentStep, steps ]);
    const canGoPrevious = useMemo(() => currentStep.canMovePrevious ? currentStep.canMovePrevious(formData) : previousStep !== undefined, [ currentStep, previousStep, formData ]);
    const previousStepFunc = useMemo(() => previousStep ? () => setCurrentStep(previousStep) : undefined, [ previousStep ]);
    const nextStep = useMemo(() => currentStepIndex + 1 < steps.length ? steps[currentStepIndex+1] : undefined, [ currentStep, steps ]);
    const isLastStep = useMemo(() => nextStep === undefined, [ nextStep ]);
    const canGoNext = useMemo(() => currentStep.canMoveNext ? currentStep.canMoveNext(formData) : nextStep !== undefined || isLastStep, [ currentStep, nextStep, formData ]);
    const nextStepFunc = useMemo(() => nextStep 
        ? async () => setCurrentStep(nextStep) 
        : async () => await onSubmit(formData)
    , [ nextStep ]);
    const hideNavigation = useMemo(() => currentStep.hideNavigation && currentStep.hideNavigation(formData), [ currentStep, formData ]);

    const StepView = useMemo(() => currentStep.element, [ currentStep ]);
    return (<>
        <div className="d-flex flex-row multistep-progress">
            {steps.map(step => {
                const classes: string[] = [ 'flex-fill', 'step' ];
                if(step === currentStep) {
                    classes.push('active');
                }
                const isAfterCurrentStep = steps.indexOf(step) > steps.indexOf(currentStep);
                if(isAfterCurrentStep) {
                    classes.push('disabled');
                }
                return (
                    <div key={step.title} 
                        className={classes.join(' ')}
                        onClick={!isAfterCurrentStep ? () => setCurrentStep(step) : undefined}
                    >
                        {step.title}
                    </div>
                );
            })}
        </div>
        <StepView
            formData={formData}
            onChange={onChange}
        />
        {!hideNavigation
        ? <PreviousNextButtons
            canGoPrevious={canGoPrevious}
            onPrevious={previousStepFunc}
            canGoNext={canGoNext}
            onNext={nextStepFunc}
            nextText={isLastStep ? submitButtonText ?? resolveText("Submit") : undefined}
            nextButtonSize={isLastStep ? 'lg' : undefined}
        /> : null}
    </>);

}