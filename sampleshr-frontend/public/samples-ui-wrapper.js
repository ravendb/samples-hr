function canRenderWebComponent() {
    return (
        "customElements" in window &&
        "define" in window.customElements &&
        "attachShadow" in Element.prototype &&
        "ShadowRoot" in window
    );
}

if (!canRenderWebComponent()) {
    throw new Error("This browser does not support web component!");
}


class SamplesUIWrapper extends HTMLElement {
    baseUrl = ".";

    constructor() {
        super();
        this.attachShadow({ mode: "open" });

        const stylesElement = document.createElement("link");
        stylesElement.setAttribute("rel", "stylesheet");
        stylesElement.setAttribute("href", `${this.baseUrl}/samples-ui-wrapper.css`);

        const wrapperElement = document.createElement("div");
        wrapperElement.classList.add("samples-ui-wrapper");
        wrapperElement.innerHTML = `
            <div class="header">
                <a href="https://ravendb.net" target="_blank">
                    <img src="${this.baseUrl}/assets/ravendb-logo.svg" alt="RavenDB" class="ravendb-logo" />
                </a>
                <a class="badge-link source-link" href="https://github.com/ravendb" target="_blank">
                    <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 12 12" fill="var(--icon-color)" style="scale: 1.25;">
                        <path d="M6.00007 1.60547C3.51507 1.60547 1.5 3.61999 1.5 6.10524C1.5 8.09335 2.78939 9.78005 4.57737 10.3751C4.80224 10.4167 4.88491 10.2774 4.88491 10.1586C4.88491 10.0513 4.8807 9.69683 4.8788 9.32083C3.62681 9.59307 3.36265 8.78991 3.36265 8.78991C3.15802 8.26983 2.86301 8.1315 2.86301 8.1315C2.45474 7.85221 2.89386 7.85796 2.89386 7.85796C3.34562 7.88969 3.58359 8.3217 3.58359 8.3217C3.985 9.00959 4.63631 8.8107 4.89319 8.69573C4.93352 8.40495 5.0501 8.20638 5.17889 8.09402C4.17931 7.98028 3.12861 7.59439 3.12861 5.87021C3.12861 5.37895 3.30443 4.97751 3.59232 4.66241C3.5456 4.54903 3.39146 4.09139 3.63585 3.47154C3.63585 3.47154 4.01383 3.35068 4.8737 3.93286C5.23261 3.83313 5.61757 3.78314 6.00005 3.7814C6.38239 3.78314 6.76765 3.83313 7.1273 3.93286C7.98629 3.35067 8.36354 3.47154 8.36354 3.47154C8.60851 4.09139 8.4545 4.54903 8.4078 4.66241C8.69626 4.97753 8.87093 5.37896 8.87093 5.87021C8.87093 7.59854 7.81819 7.97905 6.81613 8.09044C6.97754 8.23009 7.12133 8.50391 7.12133 8.92371C7.12133 9.5258 7.11609 10.0104 7.11609 10.1586C7.11609 10.2784 7.19714 10.4187 7.42522 10.3745C9.21223 9.77882 10.5 8.09272 10.5 6.10524C10.5 3.61999 8.48525 1.60547 6.00007 1.60547Z"/>
                    </svg>
                    View the source
                </a>
            </div>
            <main>
                <slot></slot>
            </main>
            <div class="welcome-toast">
                <div class="welcome-toast-content">
                    <div class="welcome-toast-close" onclick="document.querySelector('samples-ui-wrapper').closeWelcomeToast()">
                        <svg xmlns="http://www.w3.org/2000/svg" width="9" height="9" viewBox="0 0 9 9" fill="var(--text-toast)" style="scale: 1.25;">
                            <path d="M5.39739 4.50001L7.24739 2.66251L6.24739 1.67501L4.40989 3.51251L2.57239 1.66251L1.57239 2.66251L3.42239 4.50001L1.58489 6.33751L2.57239 7.32501L4.40989 5.48751L6.24739 7.33751L7.24739 6.33751L5.39739 4.50001Z"/>
                        </svg>
                    </div>
                    <div class="welcome-toast-title">Welcome to RavenDB Sample App</div>
                    <p class="welcome-toast-description">
                        This project demonstrates how to build apps with RavenDB features like document storage, indexing, and real-time subscriptions.
                    </p>
                    <a class="welcome-toast-link" href="https://ravendb.net/cloud" target="_blank">
                        Try it yourself on RavenDB Cloud
                        <svg width="8" height="9" viewBox="0 0 8 9" xmlns="http://www.w3.org/2000/svg" fill="var(--text-toast)" style="scale: 1.25;">
                            <path d="M0.569336 7.26527L5.73282 2.10178H2.96975L2.96959 0.889648H7.79053L7.79037 5.70978L6.57824 5.70963V2.94655L1.41444 8.11035L0.569336 7.26527Z"/>
                        </svg>
                    </a>
                </div>
            </div>
            <div class="footer">
                <div class="resources">
                    <a>
                        Resources
                    </a>
                    <a class="badge-link" href="https://docs.ravendb.net" target="_blank">
                        <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 12 12" fill="var(--icon-color)" style="scale: 1.25;">
                            <path d="M6.76876 0.881226H2.13751V11.1375H9.84376V3.95623L6.76876 0.881226ZM6.26251 1.64998L9.07501 4.46248H6.26251V1.64998ZM8.10001 9.31873H3.90001V8.28748H8.11876V9.31873H8.10001ZM8.10001 7.27498H3.90001V6.24373H8.11876V7.27498H8.10001Z"/>
                        </svg>
                        Docs
                    </a>
                    <a class="badge-link" href="https://ravendb.net/community" target="_blank">
                        <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 12 12" fill="var(--icon-color)" style="scale: 1.25;">
                            <path d="M6.21703 5.71095C6.56095 5.36703 6.9523 5.21286 7.42667 5.21286C7.90104 5.21286 8.32797 5.36703 8.63631 5.71095C8.98023 6.05486 9.1344 6.44622 9.1344 6.92059C9.1344 7.39496 8.98023 7.82189 8.63631 8.13023C8.29239 8.47415 7.90104 8.62832 7.42667 8.62832C6.9523 8.62832 6.52537 8.47415 6.21703 8.13023C5.87311 7.78631 5.71894 7.39496 5.71894 6.92059C5.71894 6.44622 5.87311 6.03115 6.21703 5.71095ZM10.854 11.6168H3.99935V11.0357C3.99935 10.6681 4.21282 10.3479 4.62789 10.0633C5.05483 9.77866 5.52919 9.5652 6.02728 9.45846C6.56095 9.32801 6.99974 9.26872 7.42667 9.26872C7.8536 9.26872 8.2924 9.31615 8.82606 9.45846C9.35973 9.60077 9.83409 9.80238 10.2254 10.0633C10.6524 10.3479 10.854 10.6681 10.854 11.0357V11.6168Z"/>
                            <path fill-rule="evenodd" clip-rule="evenodd" d="M5.80742 0.383179C7.05203 0.383179 8.17604 0.899504 8.97819 1.73096L8.98008 1.73275C9.81034 2.52088 10.328 3.63508 10.328 4.87021C10.328 4.89867 10.3278 4.92708 10.3273 4.95121C10.3278 4.97841 10.328 5.00567 10.328 5.03298C10.328 5.58887 10.2232 6.12026 10.032 6.60862C9.97348 6.06714 9.75846 5.57974 9.37172 5.15763C9.3756 5.08947 9.37815 5.0208 9.37937 4.9516L9.3793 4.94705C9.37369 4.62795 9.33864 4.31603 9.28233 4.04661H7.74186C7.75266 4.13679 7.76196 4.22743 7.76975 4.31865C7.65548 4.30522 7.5408 4.2988 7.42667 4.2988C7.23315 4.2988 7.04581 4.31705 6.8651 4.35418C6.85602 4.24689 6.84501 4.14369 6.83233 4.04661H4.64357L4.64796 4.00508C4.60815 4.31336 4.58524 4.62695 4.58014 4.9516L4.5802 4.95744C4.58455 5.27549 4.6068 5.58923 4.64171 5.85658H5.01609C4.87069 6.19277 4.80488 6.55429 4.80488 6.92059C4.80488 7.57824 5.0157 8.1646 5.46175 8.66165C5.00214 8.80161 4.57136 9.01021 4.18049 9.26354C3.53586 9.03273 2.96051 8.65576 2.49401 8.17222L2.49216 8.17048C1.66296 7.38245 1.146 6.26896 1.146 5.03472C1.146 5.00565 1.14628 4.97665 1.14679 4.952C1.14628 4.9248 1.14603 4.89753 1.14603 4.87021C1.14603 3.63509 1.66373 2.52088 2.49588 1.73097L2.49713 1.72966C3.29803 0.899515 4.42204 0.38319 5.66665 0.38319C5.69121 0.38319 5.71572 0.383383 5.73655 0.383739C5.76013 0.383369 5.78375 0.383179 5.80742 0.383179ZM7.52648 3.1211H8.86064L8.86249 3.11923L8.85376 3.10215C8.44097 2.36154 7.75608 1.79332 6.91053 1.52624C7.17533 2.00902 7.38098 2.52997 7.52648 3.1211ZM6.58588 3.06989C6.38454 2.42221 6.09122 1.81505 5.73703 1.28873L5.75286 1.26367C5.38285 1.81505 5.08953 2.42221 4.87496 3.11923H6.5991L6.58588 3.06989ZM2.09476 4.95614C2.10036 5.27524 2.13542 5.58716 2.19172 5.85658H3.72287C3.68828 5.56764 3.669 5.27383 3.66583 4.97212V4.93107L3.66586 4.92668C3.66914 4.61566 3.69006 4.30887 3.72367 4.04661H2.19172L2.19732 4.01389C2.13542 4.31602 2.10036 4.62795 2.09469 4.9516L2.09476 4.95614ZM2.61441 6.78956L2.62033 6.80126C3.03009 7.54334 3.71243 8.11387 4.55607 8.38441C4.29127 7.90163 4.08562 7.38069 3.94012 6.78956H2.61441ZM2.61156 3.1267H3.94012L3.94758 3.11923L3.95715 3.07427C4.09719 2.51694 4.30899 1.98812 4.56521 1.53131L4.54142 1.53781C3.71861 1.79862 3.03302 2.36776 2.61156 3.1267Z" />
                        </svg>
                        Community
                    </a>
                    <a class="badge-link" href="https://demo.ravendb.net" target="_blank">
                        <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 12 12" fill="var(--icon-color)" style="scale: 1.25;">
                            <path d="M11.6836 4.92464V4.67505L5.99797 1.93359L0.316406 4.67099V4.92667L5.99797 7.66406L11.6836 4.92464Z"/>
                            <path d="M2.28516 8.01056L5.7119 9.65625H6.2881L9.7047 8.01461C9.7047 8.01461 9.71079 8.00044 9.71484 7.99235V6.375L6 8.16036L2.28516 6.375V8.01056Z"/>
                            <path d="M11.0844 8.8749V5.69531L10.6304 5.92214V8.8749L10.3359 9.76599L10.7818 10.3594H10.9331L11.3789 9.76599L11.0844 8.8749Z"/>
                        </svg>
                        Tutorials
                    </a>
                </div>
                <div class="powered-by">
                    <div>
                        Powered by <a href="https://ravendb.net" class="ravendb-link" target="_blank">RavenDB</a>
                    </div>
                    <div class="toggle-resources">
                        <div onclick="document.querySelector('samples-ui-wrapper').hideResources()" class="hide-resources">
                            Hide
                            <svg width="9" height="6" viewBox="0 0 9 6" xmlns="http://www.w3.org/2000/svg" fill="#7F7D99" style="scale: 1.25;">
                                <path d="M8.63049 1.61304L4.64905 5.60815L0.669556 1.62866L1.49084 0.807373L4.651 3.9519L7.8092 0.791748L8.63049 1.61304Z"/>
                            </svg>
                        </div>
                        <div onclick="document.querySelector('samples-ui-wrapper').showResources()" class="show-resources">
                            Show
                            <svg width="9" height="6" viewBox="0 0 9 6" xmlns="http://www.w3.org/2000/svg" fill="#7F7D99" style="scale: 1.25;">
                                <path d="M0.669556 4.78687L4.651 0.791748L8.63049 4.77124L7.8092 5.59253L4.64905 2.448L1.49084 5.60815L0.669556 4.78687Z"/>
                            </svg>
                        </div>
                    </div>
                </div>
            </div>
        `;

        this.shadowRoot.appendChild(stylesElement);
        this.shadowRoot.appendChild(wrapperElement);
    }

    connectedCallback() {
        this.validateAttributes();

        this.setTheme(this.getAttribute("theme"));
        this.shadowRoot.querySelector(".source-link").href = this.getAttribute("sourceLink");
    }

    validateAttributes() {
        const requiredAttributes = ["sourceLink"];

        for (const attribute of requiredAttributes) {
            const value = this.getAttribute(attribute);

            if (value == null) {
                console.error(`Attribute '${attribute}' is required`);
            }
        }
    }

    hideResources() {
        this.shadowRoot.querySelector(".resources").style.display = "none";
        this.shadowRoot.querySelector(".show-resources").style.display = "flex";
        this.shadowRoot.querySelector(".hide-resources").style.display = "none";
    }

    showResources() {
        this.shadowRoot.querySelector(".resources").style.display = "flex";
        this.shadowRoot.querySelector(".show-resources").style.display = "none";
        this.shadowRoot.querySelector(".hide-resources").style.display = "flex";
    }

    closeWelcomeToast() {
        this.shadowRoot.querySelector(".welcome-toast").style.display = "none";
    }

    setTheme(theme) {
        const wrapper = this.shadowRoot.querySelector(".samples-ui-wrapper");
        wrapper.classList.remove("light", "dark");
        wrapper.classList.add(theme == null ? "light" : theme);
    }
}

customElements.define("samples-ui-wrapper", SamplesUIWrapper);
