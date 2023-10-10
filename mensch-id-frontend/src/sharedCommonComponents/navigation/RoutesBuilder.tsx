import { Routes, Route } from 'react-router-dom';
import { RouteDefinition } from '../types/frontendTypes';
import { ReactNode } from 'react';

interface RoutesBuilderProps {
    routeDefinitions: RouteDefinition[];
    wrapper?: (children: ReactNode) => ReactNode;
    containerBuilder: (children: ReactNode) => ReactNode;
}

export const RoutesBuilder = (props: RoutesBuilderProps) => {

    return (
    <Routes>
        {props.routeDefinitions.map(routeDefinition => {
            const containeredElement = routeDefinition.usesCustomLayout 
                ? routeDefinition.element
                : props.containerBuilder(routeDefinition.element);
            return (
                <Route
                    key={routeDefinition.path}
                    path={routeDefinition.path} 
                    element={props.wrapper
                        ? props.wrapper(containeredElement)
                        : containeredElement
                    } 
                />
            );
        })}
    </Routes>);

}